﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;

namespace Tasks
{
    class FarKill : Task
    {
        public override TaskType type { get; } = TaskType.FarKill;

        int distance = 100;
        Vector3[] playerGroundPositions;
        bool active = false;

        public override string GetDescription()
        {
            return $"Kill an enemy {distance}m from where you left the ground";
            //return "Kill an enemy 100m from your last ground point";
        }

        protected override void SetHooks(int numPlayers)
        {
            Debug.Log($"Set hooks in FarKill. {numPlayers} players");
            base.SetHooks(numPlayers);

            GlobalEventManager.onCharacterDeathGlobal += OnKill;

            playerGroundPositions = new Vector3[numPlayers];
            for (int i = 0; i < totalNumberPlayers; i++)
            {
                // initialize in the off chance a player is never not airborne before they get a kill
                CharacterMaster master = TasksPlugin.GetPlayerCharacterMaster(i);
                playerGroundPositions[i] = master.GetBody().corePosition;
            }
            // Make active before starting the coroutine.
            // making this false (when the task ends) stops the coroutine
            active = true;

            // just check every frame if the player is airborne
            // only update their position if grounded
            TasksPlugin.instance.StartCoroutine(CalculatePositions());
        }

        protected override void Unhook()
        {
            if (!active)
                return;
            active = false;

            GlobalEventManager.onCharacterDeathGlobal -= OnKill;

            base.Unhook();
        }

        void OnKill(DamageReport report)
        {
            if (report.attackerMaster.playerCharacterMasterController is null) return;
            int playerNum = TasksPlugin.GetPlayerNumber(report.attackerMaster);

            float dist = Vector3.Distance(report.victimBody.corePosition, playerGroundPositions[playerNum]);
            if(dist > distance)
            {
                CompleteTask(playerNum);
            }
        }

        IEnumerator CalculatePositions()
        {
            while (active)
            {
                for (int i = 0; i < totalNumberPlayers; i++)
                {
                    CharacterBody body = TasksPlugin.GetPlayerCharacterMaster(i).GetBody();
                    if (body.characterMotor.isGrounded)
                    {
                        playerGroundPositions[i] = body.corePosition;
                    }
                }
                yield return null;
            }
        }
    }
}
