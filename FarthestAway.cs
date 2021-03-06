﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;

namespace Tasks
{
    class FarthestAway : Task
    {
        protected new string description { get; } = "Farthest away in 20s wins";
        /*
        public override string AchievementIdentifier { get; } = "SOLRUN_TASKS_FARTHEST_AWAY_ACHIEVEMENT_ID"; // delete this from XML if there 
        public override string UnlockableIdentifier { get; } = "SOLRUN_TASKS_FARTHEST_AWAY_REWARD_ID"; // Delete me from XML too
        // XML: C:\Program Files (x86)\Steam\userdata\Some Numbers\632360\remote\UserProfiles\MoreNumbers.xml
        // I think all this does is hide it in the log until you have the prereq. You could still complete it (except most prereqs seem to be characters)
        public override string PrerequisiteUnlockableIdentifier { get; } = "";
        public override string AchievementNameToken { get; } = "SOLRUN_TASKS_FARTHEST_AWAY_ACHIEVEMENT_NAME"; // Fine to have in the XML
        public override string AchievementDescToken { get; } = description; // plain English
        public override string UnlockableNameToken { get; } = ""; // plain English
        */
        public override TaskType type { get; } = TaskType.FarthestAway;
        protected override string name { get; } = "Farthest From Spawn";

        Vector3[] startPositions;
        bool active = false;

        public override bool CanActivate(int numPlayers)
        {
            return numPlayers > 1;
        }

        public override string GetDescription()
        {
            return description;
        }

        protected override void SetHooks(int numPlayers)
        {
            Debug.Log($"Set Hooks in FarthestAway. {numPlayers} players");

            base.SetHooks(numPlayers);


            if (startPositions is null || startPositions.Length != numPlayers)
            {
                startPositions = new Vector3[numPlayers];
            }

            for (int i = 0; i < startPositions.Length; i++)
            {
                startPositions[i] = TasksPlugin.GetPlayerCharacterMaster(i).GetBody().transform.position;

                // are they up in the air?
                // doesn't seem to be. 
                // FarthestAway(0): (13.3, 4.0, 33.1) -> (15.1, 4.0, -12.0) = 45.13267.  titan plains
                // FarthestAway(0): (-4.8, -149.2, 97.0) -> (203.1, -133.6, -71.4) = 268.0448 swamp
                // FarthestAway(0): (229.0, 30.2, -64.3) -> (44.0, 3.8, -34.2) = 189.251. snow map
            }

            TasksPlugin.instance.StartCoroutine(EndTask());
            active = true;
        }

        protected override void Unhook()
        {
            if (!active)
                return;
            active = false;


            base.Unhook();
        }

        void UpdateProgress(int time)
        {
            // I could have this just be the timer. So it just fills up over the 20s
            // or I could have that AND show the relative difference between the players.
            // but then I would have to calculate distance every second. Which isn't that big of a deal
            // I think the task would feel better with the progress to see if you're winning or it's close, etc.
            // isntead of two players having different intensity. One is super try hard bc he thinks the other is right on his heels and everyone else is not trying
            float[] currentDist = new float[startPositions.Length];
            float maxDist = 0;
            
            if (time > 0)
            {
                for (int i = 0; i < startPositions.Length; i++)
                {
                    currentDist[i] = Vector3.Distance(startPositions[i], TasksPlugin.GetPlayerCharacterMaster(i).GetBody().transform.position);
                    if (currentDist[i] > maxDist)
                    {
                        maxDist = currentDist[i];
                    }
                }
            }
            if (maxDist > 0)
            {
                for (int i = 0; i < progress.Length; i++)
                {
                    progress[i] = (time * (currentDist[i] / maxDist)) / 20;
                }
            }
            else
            {
                for (int i = 0; i < progress.Length; i++)
                {
                    // if time is 0, it skips the distance calc so maxDist is 0. Reset the progress bar
                    progress[i] = 0;
                }
            }
            base.UpdateProgress(progress);
        }

        IEnumerator EndTask()
        {
            for (int i = 0; i < 20; i++)
            {
                yield return new WaitForSeconds(1);
                UpdateProgress(i+1);
            }
            //yield return new WaitForSeconds(20);
            Evaluate();
        }

        void Evaluate()
        {
            float mostDist = 0;
            int winner = 0;


            for (int i = 0; i < startPositions.Length; i++)
            {
                Vector3 endPos = TasksPlugin.GetPlayerCharacterMaster(i).GetBody().transform.position;
                float dist = Vector3.Distance(startPositions[i], endPos);

                if (dist > mostDist)
                {
                    mostDist = dist;
                    winner = i;
                }
                //Chat.AddMessage($"FarthestAway({i}): {startPositions[i]} -> {endPos} = {dist}. Winner: {winner} with {mostDist}");
            }

            CompleteTask(winner);
            ResetProgress();
        }

    }
}
