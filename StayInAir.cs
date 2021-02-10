﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Tasks
{
    class StayInAir : Task
    {
        public static new string description { get; } = "Stay airborne for 10 seconds";

        public override string AchievementIdentifier { get; } = "SOLRUN_TASKS_STAY_AIRBORNE_ACHIEVEMENT_ID"; // delete this from XML if there
        public override string UnlockableIdentifier { get; } = "SOLRUN_TASKS_STAY_AIRBORNE_REWARD_ID"; // Delete me from XML too
        // I think all this does is hide it in the log until you have the prereq. You could still complete it (except most prereqs seem to be characters)
        public override string PrerequisiteUnlockableIdentifier { get; } = "";
        public override string AchievementNameToken { get; } = "SOLRUN_TASKS_STAY_AIRBORNE_ACHIEVEMENT_NAME"; // Fine to have in the XML
        public override string AchievementDescToken { get; } = description; // plain English
        public override string UnlockableNameToken { get; } = ""; // plain English

        protected override TaskType type { get; } = TaskType.StayInAir;
        protected override string name { get; } = "Stay Airborne";

        CharacterMotor[] motors;
        CharacterBody[] bodies;

        float[] timeInAir;
        float timeToStayInAir = 10;

        protected override void SetHooks(int numPlayers)
        {
            Chat.AddMessage($"Set Hooks in Stay Airborne. {numPlayers} players");

            base.SetHooks(numPlayers);

            if (timeInAir is null || timeInAir.Length != numPlayers)
            {
                timeInAir = new float[numPlayers];
            }

            if(motors is null || motors.Length != numPlayers)
            {
                motors = new CharacterMotor[numPlayers];
            }

            if(bodies is null || motors.Length != numPlayers)
            {
                bodies = new CharacterBody[numPlayers];
            }

            Reset();
            SetupBodies();
            
            // This is how the merc's stay in the air achieve works
            RoR2Application.onFixedUpdate += AirborneFixedUpdate;
        }

        protected override void Unhook()
        {
            RoR2Application.onFixedUpdate -= AirborneFixedUpdate;

            Reset();

            base.Unhook();
        }

        
        private void AirborneFixedUpdate()
        {
            // does this break when one player dies?
            for (int i = 0; i < timeInAir.Length; i++)
            {
                timeInAir[i] = ((motors[i] && !motors[i].isGrounded && !bodies[i].currentVehicle) ? (timeInAir[i] + Time.fixedDeltaTime) : 0f);
                if(IsComplete(i))
                {
                    Chat.AddMessage($"Player {i} Completed StayAirborne");
                    CompleteTask(i);
                    Reset();
                }
            }
        }

        override protected bool IsComplete(int playerNum)
        {
            return timeInAir[playerNum] >= timeToStayInAir;
        }

        void Reset()
        {
            for (int i = 0; i < timeInAir.Length; i++)
            {
                timeInAir[i] = 0;
            }
        }

        void SetupBodies()
        {
            for (int i = 0; i < motors.Length; i++)
            {
                CharacterMaster current = TasksPlugin.GetPlayerCharacterMaster(i);
                bodies[i] = current.GetBody();
                motors[i] = current.GetBody().characterMotor;
            }
        }
    }
}
