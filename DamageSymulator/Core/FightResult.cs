
namespace Plazacraft.HOMM3.DamageSymulator.Core
{
    public class FightResult
    {
        public FightResult()
        {

        }
        public int DamageMin
        { get; set;}
        public int DamageMax
        { get; set;}

        public int DamageAvg
        { get; set;}

        public int DamageLeftMin
        { get; set;}
        public int DamageLeftMax
        { get; set;}

        public int DamageLeftAvg
        { get; set;}


        public int KilledMin
        {get; set;}

        public int KilledMax
        {get; set;}

        public int KilledAvg
        {get; set;}

        public int Retaliations
        {get; set;} = 0;

        public bool DoubleAttack
        {get; set;} = false;                
    }
}