﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace Tasks
{
    class AirKills : Task
    {
        protected new string description { get; } = "Get 3 kills whilst airborne";

        public override TaskType type { get; } = TaskType.AirKills;
        protected override string name { get; } = "Air Kills";

        int[] kills;
        int killsNeeded = 3;


        List<CharacterMotor.HitGroundDelegate> groundDelegateList = new List<CharacterMotor.HitGroundDelegate>();


        public override string GetDescription()
        {
            return description;
        }

        public override string GetWinMessage(int winningPlayer)
        {
            return $"{GetStylizedName(winningPlayer)} completed {GetStylizedTaskName(name)}.";
        }

        protected override void SetHooks(int numPlayers)
        {
            //Language.currentLanguage.SetStringByToken(AchievementNameToken, "Air Kills");
            UnityEngine.Debug.Log($"Set Hooks in AirKills. {numPlayers} players");
            

            base.SetHooks(numPlayers);
            kills = new int[numPlayers];

            GlobalEventManager.onCharacterDeathGlobal += OnKill;
          
            
            for (int i = 0; i < totalNumberPlayers; i++)
            {
                CharacterMaster current = TasksPlugin.GetPlayerCharacterMaster(i);
                if (current is null) break;
                if (current.GetBody() is null) break;
                if (current.GetBody().characterMotor is null) break;

                //Chat.AddMessage($"Set hooks for player {i}");
                // is it a weird timing thing?
                // like when onHitGround eventually gets called, it just finds what i was last instead of what it was when this was first added. Like lazy evaluation
                // I think I was right. When I set i=5 after this, when I hit the ground, it said player 6 did it (which is what i was last)
                // Neat. This fixes it.
                // why: https://answers.unity.com/questions/908847/passing-a-temporary-variable-to-add-listener.html
                // something something scope. a for loop is compiled into int i=0; while() {}
                // so the i isn't inside the loop like this temp is.
                int tempInt = i;

                //Chat.AddMessage($"Hook Status. Player {tempInt} CharacterMaster: {TasksPlugin.GetPlayerCharacterMaster(i)} Body: {TasksPlugin.GetPlayerCharacterMaster(i).GetBody()} Motor: {TasksPlugin.GetPlayerCharacterMaster(i).GetBody().characterMotor}");

                // to get it to remove itself, I need to store the delegate in a list
                CharacterMotor.HitGroundDelegate myDel = (ref CharacterMotor.HitGroundInfo _) => PlayerHitGround(tempInt);
                groundDelegateList.Add(myDel);
                current.GetBody().characterMotor.onHitGround += groundDelegateList[tempInt];
                // this version works, but I can't unsub it
                //TasksPlugin.GetPlayerCharacterMaster(i).GetBody().characterMotor.onHitGround += (ref CharacterMotor.HitGroundInfo _) => PlayerHitGround(tempInt);
            }
        }

        protected override void Unhook()
        {
            GlobalEventManager.onCharacterDeathGlobal -= OnKill;

            for (int i = 0; i < totalNumberPlayers; i++)
            {
                CharacterMaster current = TasksPlugin.GetPlayerCharacterMaster(i);
                if (current is null) break;
                if (current.GetBody() is null) break;
                if (current.GetBody().characterMotor is null) break;
                // I don't think I need to worry about unsubscribing when one of the players dies
                // Because onHitGround is probably destroyed/reset anyway.

                // delegate list is created when it's hooked up. This might break if unhook is called before setHooks
                int tempInt = i;
                //Chat.AddMessage($"Unhook Status. Player {tempInt} CharacterMaster: {TasksPlugin.GetPlayerCharacterMaster(i)} Body: {TasksPlugin.GetPlayerCharacterMaster(i).GetBody()} Motor: {TasksPlugin.GetPlayerCharacterMaster(i).GetBody().characterMotor}");
                //Chat.AddMessage()

                // seems like this breaks if one of the players is dead
                current.GetBody().characterMotor.onHitGround -= groundDelegateList[tempInt];
            }

            // if the task is ending, but someone didn't complete it
            ResetAllKills();

            base.Unhook();
        }

        void UpdateProgress()
        {
            for (int i = 0; i < progress.Length; i++)
            {
                progress[i] = (float)kills[i] / killsNeeded;
            }
            base.UpdateProgress(progress);
        }

        protected override bool IsComplete(int playerNum)
        {
            return kills[playerNum] >= killsNeeded;
        }

        public void OnKill(DamageReport damageReport)
        {
            if (damageReport is null) return;
            if (damageReport.attackerMaster is null) return;
            if (damageReport.attackerMaster.playerCharacterMasterController is null) return;

            //if (damageReport.victimMaster is null) return;
            //Chat.AddMessage(String.Format("Killer: {0} Me: {1} Victim: {2}", damageReport.attackerMaster.ToString(), damageReport.victimMaster.ToString(), ownerCached.localUser.cachedMaster.ToString()));
            //[Info: Unity Log] Killer: CommandoMaster(Clone)(RoR2.CharacterMaster) Me: LemurianMaster(Clone)(RoR2.CharacterMaster) Victim: CommandoMaster(Clone)(RoR2.CharacterMaster)
            // Did I kill it?
            int playerNum = TasksPlugin.GetPlayerNumber(damageReport.attackerMaster);
            
            // seems easier than reworking Airborne()
            if(!damageReport.attackerMaster.GetBody().characterMotor.isGrounded)
            {
                kills[playerNum]++;
                UpdateProgress();
                if(IsComplete(playerNum))
                {
                    //Chat.AddMessage($"Player {playerNum} Completed AirKills");
                    CompleteTask(playerNum);
                    // What about getting 2nd place?
                    ResetAllKills();
                }
            }
        }

        void PlayerHitGround(int playerNum)
        {
            //Chat.AddMessage($"Player {playerNum} landed");
            UpdateProgress();
            kills[playerNum] = 0;
        }

        void ResetAllKills()
        {
            if (kills is null)
                return;
            for (int i = 0; i < kills.Length; i++)
            {
                kills[i] = 0;
            }
            ResetProgress();
        }
    }
}
