using System;
using Microsoft.SPOT;

namespace Pathfinder
{
    class TurnAngle
    {

        double incident_angle;
        double attack_angle;
        double change_amount;
        double modified_attack;
        double turn_angle;

        bool change_direction;
        bool turn_direction;



        public void Set(double att, double inc)
        {
            attack_angle = att;
            incident_angle = inc;
            CalculateTurn();
        }
        private void CalculateTurn()
        {

            //getChange
            if (incident_angle > 180)
            {
                change_amount = 360 - incident_angle;
                change_direction = true;
            }
            else
            {
                change_amount = incident_angle;
                change_direction = false;
            }


            //getModifiedAttack
            if (change_direction)
            {
                modified_attack = attack_angle + change_amount;
            }
            else
            {
                modified_attack = attack_angle - change_amount;
            }

            if (modified_attack > 360)
            {
                modified_attack = modified_attack - 360;
            }



            //getTurnAngle
            if (modified_attack > 180)
            {
                turn_direction = false;
                turn_angle = 360 - modified_attack;
            }
            else if (modified_attack < 0)
            {
                turn_direction = false;
                turn_angle = 0 - (modified_attack);
            }
            else
            {
                turn_direction = true;
                turn_angle = modified_attack;
            }




        }




        private void CalculateDistance()
        {

       

        }

        public double GetTurnAngle()
        {
            return (turn_angle);
        }

        public bool GetTurnDirection()
        {
            return (turn_direction);
        }
    }
}
