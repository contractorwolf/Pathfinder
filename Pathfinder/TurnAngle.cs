using System;
using Microsoft.SPOT;

namespace Pathfinder
{
    public class TurnAngle
    {
        private double incidentAngle { get; set; }
        private double attackAngle { get; set; }
        private double changeAmount { get; set; }
        private double modifiedAttack { get; set; }
        private bool changeDirection { get; set; }


        public double CurrentTurnAngle { get; set; }
        public bool CurrentTurnDirection { get; set; }

        public int CurrentServoAngle { get; set; }




        public void Set(double att, double inc)
        {
            attackAngle = att;
            incidentAngle = inc;
            CalculateTurn();
        }
        private void CalculateTurn()
        {

            //getChange
            if (incidentAngle > 180)
            {
                changeAmount = 360 - incidentAngle;
                changeDirection = true;
            }
            else
            {
                changeAmount = incidentAngle;
                changeDirection = false;
            }

            //getModifiedAttack
            if (changeDirection)
            {
                modifiedAttack = attackAngle + changeAmount;
            }
            else
            {
                modifiedAttack = attackAngle - changeAmount;
            }

            if (modifiedAttack > 360)
            {
                modifiedAttack = modifiedAttack - 360;
            }

            //getTurnAngle
            if (modifiedAttack > 180)
            {
                CurrentTurnDirection = false;
                CurrentTurnAngle = 360 - modifiedAttack;
            }
            else if (modifiedAttack < 0)
            {
                CurrentTurnDirection = false;
                CurrentTurnAngle = 0 - (modifiedAttack);
            }
            else
            {
                CurrentTurnDirection = true;
                CurrentTurnAngle = modifiedAttack;
            }


            CurrentServoAngle = GetServo();
        }


        public double GetTurnAngle()
        {
            return (CurrentTurnAngle);
        }

        public bool GetTurnDirection()
        {
            return (CurrentTurnDirection);
        }

        public int GetServo()
        {
            int servoDirection = 90;

            if (CurrentTurnDirection)
            {
                servoDirection = servoDirection - (int)CurrentTurnAngle;
            }
            else
            {
                servoDirection = servoDirection + (int)CurrentTurnAngle;
            }

            if (servoDirection > 180) servoDirection = 180;
            if (servoDirection < 0) servoDirection = 0;

            return (servoDirection);
        }



    }
}
