using System.IO;
using System.Data;
using System.Collections.Generic;

namespace Plazacraft.HOMM3.DamageSymulator.Core
{

    public class FightSimulation
    {

        private Fight _fight;
        public Fight Fight
        {
            get { return _fight;}
        }

        private World _world;
        public World World
        { 
            get { return _world;}
        }

        public FightSimulation(World world, TextReader fightReader)
        {
            _fight = Helper.DeserializeXML<Fight>(fightReader);
            _world = world;

        }
        public FightSimulation(World world, string fight)
        {
            _fight = Helper.DeserializeXML<Fight>(fight);
            _world = world;
        }

        public FightSimulation(World world, Fight fight)
        {
            _fight = fight;
            _world = world;
        }

        private enum SimulationType
        {
            MinimalDamage
            ,MaximalDamage
            ,AverageDamage
        }

        private int UpdateKillsCount(int currentCount, FightResult result, SimulationType type)
        {
            int count = currentCount;
            switch (type)
            {
                case SimulationType.MinimalDamage:
                    count -= result.KilledMin;
                    break;
                case SimulationType.MaximalDamage:
                    count -= result.KilledMax;
                    break;
                case SimulationType.AverageDamage:
                    count -= result.KilledAvg;
                    break;

                default:
                    break;
            }
            if (count < 0 ) 
                count = 0;

            return count;
        }

        private int GetLeftDamage(FightResult result, SimulationType type)
        {
            switch (type)
            {
                case SimulationType.MinimalDamage:
                    return result.DamageLeftMin;
                case SimulationType.MaximalDamage:
                    return result.DamageLeftMax;
                case SimulationType.AverageDamage:
                    return result.DamageLeftAvg;

                default:
                    return 0;
            }
        }


        private void MergeFightResult(FightResult f1, FightResult f2)
        {
            f1.DamageAvg += f2.DamageAvg;
            f1.DamageMax += f2.DamageMax;
            f1.DamageMin += f2.DamageMin;
            f1.KilledAvg += f2.KilledAvg;
            f1.KilledMax += f2.KilledMax;
            f1.KilledMin += f2.KilledMin;
        }


        private void CreatureFightSimulation(Fight fight, SimulationType type, FightSimulationResult.Info info, CreatureRef attackerRef, CreatureRef defenderRef, TextWriter log)
        {
            string val = string.Format("Fight simulation ({2}) between {0} and {1}", attackerRef.Name, defenderRef.Name, System.Enum.GetName(type.GetType(), type));
            string dash = new string('-', val.Length);

            log.WriteLine(dash);
            log.WriteLine(val);
            log.WriteLine(dash);

            AttackType attackType = attackerRef.AttackType[0];
            int attackerCount = attackerRef.Count;
            int attackerDamageLeft = 0;
            int defenderCount = defenderRef.Count;
            int defenderDamageLeft = 0;

            CreatureRef currentAttackerRef = attackerRef;
            CreatureRef currentDefenderRef = defenderRef;

            Hero attackerHero = Fight.Attacker;
            Hero defenderHero = Fight.Defender;

            int rounds = 0;

            do
            {
                log.WriteLine ("ROUND: {0}", rounds + 1);
                log.WriteLine("*** Strike ***");
                FightResult strike = CreatureFight(Fight, attackerHero, defenderHero, currentAttackerRef, currentDefenderRef, attackerCount, defenderCount, log, attackType, defenderDamageLeft);
                defenderCount = UpdateKillsCount(defenderCount, strike, type);
                defenderDamageLeft = GetLeftDamage(strike, type);
                FightResult counterStrike = new FightResult();

                int retaliations = strike.Retaliations;
                if (retaliations > 0 && defenderCount > 0)
                {
                    log.WriteLine("*** Counterstrike ***");
                    log.WriteLine("Conterstrike attack, atack type is always melee");
                    counterStrike = CreatureFight(Fight, defenderHero, attackerHero, currentDefenderRef, currentAttackerRef, defenderCount, attackerCount, log, AttackType.Melee, attackerDamageLeft);
                    --retaliations;
                    attackerCount = UpdateKillsCount(attackerCount, counterStrike, type);
                    attackerDamageLeft = GetLeftDamage(counterStrike, type);
                }
                if (strike.DoubleAttack && attackerCount > 0)
                {
                    log.WriteLine("*** Double Attack ***");
                    FightResult secondStrike = CreatureFight(Fight, attackerHero, defenderHero, currentAttackerRef, currentDefenderRef, attackerCount, defenderCount, log, attackType, defenderDamageLeft);
                    defenderCount = UpdateKillsCount(defenderCount, secondStrike, type);
                    defenderDamageLeft = GetLeftDamage(secondStrike, type);
                    MergeFightResult(strike, secondStrike);
                }
                if (retaliations > 0 && defenderCount > 0)
                {
                    log.WriteLine("*** Counterstrike ***");
                    log.WriteLine("Conterstrike attack, atack type is always melee");
                    FightResult secondCounterStrike = CreatureFight(Fight, defenderHero, attackerHero, currentDefenderRef, currentAttackerRef, defenderCount, attackerCount, log, AttackType.Melee, attackerDamageLeft);
                    --retaliations;
                    attackerCount = UpdateKillsCount(attackerCount, secondCounterStrike, type);
                    attackerDamageLeft = GetLeftDamage(secondCounterStrike, type);
                    MergeFightResult(counterStrike, secondCounterStrike);
                }

                if (attackerRef.AttackType[0] == AttackType.Melee || defenderRef.AttackType[0] == AttackType.Melee)
                {
                    log.WriteLine("One of the fighers did melee attack, rest of the battle is melee.");
                    attackType = AttackType.Melee;
                }

                // store info about first attack
                if (rounds == 0)
                {
                    info.Strike = strike;
                    switch (type)
                    {
                        case SimulationType.MinimalDamage:
                            info.CounterMin = counterStrike;
                            break;
                        case SimulationType.MaximalDamage:
                            info.CounterMax = counterStrike;
                            break;
                        case SimulationType.AverageDamage:
                            info.CounterAvg = counterStrike;
                            break;
                        
                        default:
                            break;
                    }

                }
                CreatureRef tmp = currentDefenderRef;
                int countTmp = defenderCount;
                int dmgLeftTmp = defenderDamageLeft;
                Hero heroTmp = defenderHero;

                currentDefenderRef = currentAttackerRef;
                currentAttackerRef = tmp;

                defenderCount = attackerCount;
                attackerCount = countTmp;

                defenderDamageLeft = attackerDamageLeft;
                attackerDamageLeft = dmgLeftTmp;

                defenderHero = attackerHero;
                attackerHero = heroTmp;

                ++rounds;

            }while (attackerCount > 0 && defenderCount > 0);
        
            FightFinalResult res = new FightFinalResult();
            if (rounds % 2 > 0)
            {
                res.AttackerLeft = defenderCount;
                res.DefenderLeft = attackerCount;

            }
            else
            {
                res.AttackerLeft = attackerCount;
                res.DefenderLeft = defenderCount;
            }
            res.Rounds = rounds;

            switch (type)
            {
                case SimulationType.MinimalDamage:
                    info.FinalMin = res;
                    break;
                case SimulationType.MaximalDamage:
                    info.FinalMax = res;
                    break;
                case SimulationType.AverageDamage:
                    info.FinalAvg = res;
                    break;
                
                default:
                    break;
            }

        }

        public FightSimulationResult Run(TextWriter log)
        {
            FightSimulationResult res = new FightSimulationResult();
            if (Fight.Attacker.Items == null)
                return res;

            foreach (object refItem in Fight.Attacker.Items)
            {
                if (refItem is CreatureRef)
                {
                    CreatureRef attackerRef = (CreatureRef)refItem;
                    if (Fight.Defender.Items == null)
                        return res;

                    foreach (object refDefItem in Fight.Defender.Items)
                    {
                        if (refDefItem is CreatureRef)
                        {
                            CreatureRef defenderRef = (CreatureRef)refDefItem;
                            FightSimulationResult.Info info = new FightSimulationResult.Info(){ Attacker = attackerRef ,Defender = defenderRef };
                            CreatureFightSimulation(Fight, SimulationType.MinimalDamage, info, attackerRef, defenderRef, log);
                            CreatureFightSimulation(Fight, SimulationType.MaximalDamage, info, attackerRef, defenderRef, log);
                            CreatureFightSimulation(Fight, SimulationType.AverageDamage, info, attackerRef, defenderRef, log);

                            res.Add(info);
                        }
                    }

                }
            }

            return res;

        }

        private class FactorInfo
        {
            public Factor Factor;
            public object Parent;

            public Reference ParentRef;

            public Dictionary<string, Param> Params;
        }

        private void AddFactor(Dictionary<FactorType, List<FactorInfo>> dict
                                , Factor [] factors
                                , Fight fight
                                , Creature attacker
                                , Creature defender
                                , AttackType attackType
                                , CreatureRef attackerRef
                                , CreatureRef defenderRef
                                , object parent
                                , Reference parentRef
                                , Dictionary<string, Param> abilityParams)
        {
            if (factors == null)
                return;

            foreach (Factor factor in factors)
            {

                bool addFactor = true;
                if (!string.IsNullOrEmpty(factor.Affected) 
                    && factor.Affected != attacker.Name)
                    addFactor = false;

                if (!string.IsNullOrEmpty(factor.Opponent)
                    && factor.Opponent != defender.Name)
                    addFactor = false;

               if (!System.Array.Exists(factor.AttackType, element => element == attackType))
                    addFactor = false;

                if (addFactor)
                    foreach (FactorType type in factor.Type)
                    {
                        if (!dict.ContainsKey(type))
                            dict.Add(type, new List<FactorInfo>());
                        
                        dict[type].Add(new FactorInfo() {Factor = factor, Parent = parent, ParentRef = parentRef, Params = abilityParams});
                    }
            }
        }

        private void AddHeroFactors(Dictionary<FactorType, List<FactorInfo>> dict
                                , Hero hero
                                , Fight fight
                                , Creature attacker
                                , Creature defender
                                , AttackType attackType
                                , CreatureRef attackerRef
                                , CreatureRef defenderRef)
        {
            int i = 0;
            foreach (object item in hero.Items)
            {
                if (item is Reference && hero.ItemsElementName[i].ToString() == "HeroAbility")
                {
                    Reference refAbility = (Reference)item;
                    HeroAbility ability = World.HeroAbilities[refAbility.Name];
                    AddFactor(dict, ability.Items, fight, attacker, defender, attackType, attackerRef, defenderRef, ability, refAbility, null);
                }
                else if  (item is Reference && hero.ItemsElementName[i].ToString() == "SecondarySkill")
                {
                    Reference refSkill = (Reference)item;
                    SecondarySkill skill = (SecondarySkill)World.SecondarySkills[refSkill.Name];
                    AddFactor(dict, skill.Items, fight, attacker, defender, attackType, attackerRef, defenderRef, skill, refSkill, null);
                    break;
                }
                ++i;
            }

        }

        private void AddCreatureFactors(Dictionary<FactorType, List<FactorInfo>> dict
                                , CreatureRef creatureRef
                                , Fight fight
                                , Creature attacker
                                , Creature defender
                                , AttackType attackType                                
                                , CreatureRef attackerRef
                                , CreatureRef defenderRef)
        {
            Dictionary<string, Param>  abilityParams = new Dictionary<string, Param> ();

            if (creatureRef.Items != null)
                foreach (object item in creatureRef.Items)
                {
                    if (item is Spell)
                    {
                        Reference refSpell = (Reference)item;
                        Spell spell = World.Spells[refSpell.Name];
                        //AddFactor(dict, spell.Items, fight, attacker, defender, attackerRef, defenderRef);
                    }
                    if (item is Param)
                    {
                        abilityParams.Add(((Param)item).Name, (Param)item);
                    }
                }

            Creature creature = World.Creatures[creatureRef.Name];
            if (creature.Items != null)
                foreach (Reference abilityRef in creature.Items)
                {
                    CreatureAbility ability = World.CreatureAbilities[abilityRef.Name];
                    AddFactor(dict, ability.Items, fight, attacker, defender, attackType, attackerRef, defenderRef, ability, abilityRef, abilityParams);
                }

        }

        private decimal EvalModificator(object modificator, FactorInfo factorInfo, Dictionary<FactorType, List<FactorInfo>> defenderFactors, Hero hero, Creature creature, TextWriter log, decimal i2)
        {
            // %HeroLevel%
            // %CreatureLevel%
            // %Immunity%
            // %CreatureAttack%
            // %CreatureDefense%
            // %I2%
            // PARAMS

            string value = string.Empty;
            decimal retValue = 0;
            RoundType round = RoundType.None;

            // get value from modificator
            if (modificator is Modificator)
            {
                value = ((Modificator)modificator).Value;
                round = ((Modificator)modificator).RoundType;
            }
            else if (modificator is LevelModificator)
            {
                LevelModificator levelModificator = (LevelModificator)modificator;
                string level = string.Empty;
                if (factorInfo.ParentRef != null)
                    level = factorInfo.ParentRef.Level;
                if (string.IsNullOrEmpty(level) || level == "0")
                    value = levelModificator.Level_0.Value;
                else if (level == "1")
                    value = levelModificator.Level_1.Value;
                else if (level == "2")
                    value = levelModificator.Level_2.Value;
                else if (level == "3")
                    value = levelModificator.Level_3.Value;
                
                round = levelModificator.RoundType;
            }
            log.WriteLine("Evaluating value '{0}' for {1}", value, creature.Name);

            // replace standard values
            value = value.Replace("%HeroLevel%", hero.Level.ToString());
            value = value.Replace("%CreatureLevel%", creature.Level.ToString());
            value = value.Replace("%CreatureAttack%", creature.Attack.ToString());
            value = value.Replace("%CreatureDefense%", creature.Defense.ToString());

            System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            value = value.Replace("%I2%", i2.ToString(nfi));

            // check immunity
            if (factorInfo.Parent is CreatureAbility)
            {
                string immunity = "0";
                CreatureAbility ability = (CreatureAbility)factorInfo.Parent;
                if (defenderFactors.ContainsKey(FactorType.ImmuneToCreatureAbility))
                {
                    List<FactorInfo> immunityFactors = defenderFactors[FactorType.ImmuneToCreatureAbility];
                    foreach (FactorInfo immunityFactor in immunityFactors)
                    {
                        if (ability.Name ==immunityFactor.Factor.CreatureAbility)
                            immunity = "1";
                    }
                }
                value = value.Replace("%Immunity%", immunity);
            }

            // params
            if (factorInfo.Parent is CreatureAbility)
            {
                CreatureAbility ability = (CreatureAbility)factorInfo.Parent;
                if (!string.IsNullOrEmpty(ability.Params))
                {
                    string [] abilityParams = ability.Params.Split(' ');
                    foreach (string param in abilityParams)
                    {
                        string val = "0";
                        if (factorInfo.Params != null && factorInfo.Params.ContainsKey(param))
                        {
                            Param refParam = factorInfo.Params[param];
                            val = refParam.Value.ToString();
                        }
                        value = value.Replace("%" + param + "%", val);
                    }
                }
            }

            // evaluate string
            log.WriteLine("Evaluating direct value '{0}' for {1}", value, creature.Name);

            DataTable dt = new DataTable();
            retValue = System.Convert.ToDecimal(dt.Compute(value, string.Empty));
            if (round == RoundType.Down)
                retValue = System.Math.Floor(retValue);
            else if (round == RoundType.Up)
                retValue = System.Math.Ceiling(retValue);
            else if (round == RoundType.Normal)
                retValue = System.Math.Round(retValue);

            log.WriteLine("Counted value '{0}' for {1}", retValue, creature.Name);

            return retValue;

        }

        private decimal ExecuteFactor(FactorType type, Dictionary<FactorType, List<FactorInfo>> factors, Dictionary<FactorType, List<FactorInfo>> defenderFactors, Hero hero, Creature creature, TextWriter log, decimal i2 = 1)
        {
            decimal value = 0;
            
            if (!factors.ContainsKey(type))
                return value;
            
            List<FactorInfo> factorInfos = factors[type];
            

            foreach (FactorInfo info in factorInfos)
            {
                log.WriteLine("Executing bonus: {0} for {1}. Factor type: {2}", info.ParentRef.Name, creature.Name, type.ToString());

                object modificator = null;

                if (info.Factor.Item is Modificator)
                    modificator = info.Factor.Item;
                else if (info.Factor.Item is Reference)
                {
                    object refModificator = World.Modificators[((Reference)info.Factor.Item).Name];
                    modificator  = (Modificator)refModificator;

                }
                else if (info.Factor.Item is LevelModificator)
                    modificator = info.Factor.Item;

                value += EvalModificator(modificator, info, defenderFactors, hero, creature, log, i2);
            }

            log.WriteLine("Factor Type: {0} evaluationg finished with value: '{1}' for {2}.", type.ToString(), value, creature.Name);

            return value;

        }

        private FightResult CreatureFight(Fight fight, Hero attackerHero, Hero defenderHero, CreatureRef attackerRef, CreatureRef defenderRef, int attackerCount, int defenderCount, TextWriter log, AttackType attackType, int damageLeft = 0)
        {
            log.WriteLine("{0} ({1}) attacks {2} ({3})", attackerRef.Name, attackerCount, defenderRef.Name, defenderCount);
            decimal I1=0, R1=0;
            decimal I2=0, I3=0, I4=0, I5=0, R2=0, R3=0, R4=0, R5=0, R6=0, R7=0, R8=0;

            int DMGbmin = 0;
            int DMGbmax = 0;

            // TODO: check for spells modification for DMGb
            Creature attacker = World.Creatures[attackerRef.Name];
            Creature defender = World.Creatures[defenderRef.Name];

            if (attackType == AttackType.Ranged
                && !System.Array.Exists(attacker.AttackType, element => element == AttackType.Ranged))
            {
                log.WriteLine("{0} cannot attack ranged. Attack type changed to Melee", attackerRef.Name);
                attackType = AttackType.Melee;
            }



            Dictionary<FactorType, List<FactorInfo>> attackerFactors = new Dictionary<FactorType, List<FactorInfo>>();
            Dictionary<FactorType, List<FactorInfo>> defenderFactors = new Dictionary<FactorType, List<FactorInfo>>();

            // Loading heroes factors
            AddHeroFactors(attackerFactors, attackerHero, fight, attacker, defender, attackType, attackerRef, defenderRef);
            AddHeroFactors(defenderFactors, defenderHero, fight, attacker, defender, attackType, attackerRef, defenderRef);

            // Loading creature factors
            AddCreatureFactors(attackerFactors, attackerRef, fight, attacker, defender, attackType, attackerRef, defenderRef);
            AddCreatureFactors(defenderFactors, defenderRef, fight, attacker, defender, attackType, attackerRef, defenderRef);


            // I1 & R1
            // basic I1 & R1
            int attack = attackerHero.Attack + attacker.Attack;
            int defense = defenderHero.Defense + defender.Defense;

            if (attacker.Terrain[0] == fight.Terrain[0])
            {
                attack += 1;
                log.WriteLine("+1 added to atack to {0} for terrain bonus", attacker.Name);
            }
            if (defender.Terrain[0] == fight.Terrain[0])
            {
                defense += 1;
                log.WriteLine("+1 added to defense to {0} for terrain bonus", defender.Name);
            }

            DMGbmin = attacker.MinDamage;
            DMGbmax = attacker.MaxDamage;
            int dmg = System.Convert.ToInt32(ExecuteFactor(FactorType.I0, attackerFactors, defenderFactors, attackerHero, attacker, log));
            if (dmg > 0)
            {
                DMGbmax += dmg;
                log.WriteLine("{0} added to DMGMax.", DMGbmax);
            }

            DMGbmin = DMGbmin * attackerCount;
            DMGbmax = DMGbmax * attackerCount;

            attack += System.Convert.ToInt32(ExecuteFactor(FactorType.I1, attackerFactors, defenderFactors, attackerHero, attacker, log));
            defense += System.Convert.ToInt32(ExecuteFactor(FactorType.R1, defenderFactors, defenderFactors, defenderHero, defender, log));

            attack = System.Convert.ToInt32(attack * (1 - ExecuteFactor(FactorType.IgnoreAttack, defenderFactors, defenderFactors, attackerHero, attacker, log)));
            defense = System.Convert.ToInt32(defense * (1 - ExecuteFactor(FactorType.IgnoreDefense, attackerFactors, defenderFactors, attackerHero, attacker, log)));

            if (attack >= defense)
            {
                I1 = (decimal)0.05 * (attack-defense);
                if (I1 > 3)
                    I1 = 3;
                log.WriteLine("Attack({1}) > Defense({2}), I1={0}", I1, attack, defense);
            }

            if (defense >= attack)
            {
                R1 = (decimal)0.025 * (defense - attack);
                if (R1 >= (decimal)0.7)
                    R1 = (decimal)0.7;
                log.WriteLine("Defense({1}) > Attack({2}), R1={0}", R1, defense, attack);
            }

            I2 = ExecuteFactor(FactorType.I2, attackerFactors, defenderFactors, attackerHero, attacker, log);
            log.WriteLine("I2 value: {0}", I2);

            I3 = ExecuteFactor(FactorType.I3, attackerFactors, defenderFactors, attackerHero, attacker, log, I2);
            log.WriteLine("I3 value: {0}", I3);

            if (attackerRef.Luck)
                I4 = 1;
            log.WriteLine("I4 (luck) value: {0}", I4);

            I5 = ExecuteFactor(FactorType.I5, attackerFactors, defenderFactors, attackerHero, attacker, log);
            log.WriteLine("I5 value: {0}", I5);

            R2 = ExecuteFactor(FactorType.R2, defenderFactors, defenderFactors, defenderHero, defender, log);
            log.WriteLine("R2 value: {0}", R2);

            R3 = ExecuteFactor(FactorType.R3, defenderFactors, defenderFactors, defenderHero, defender, log);
            log.WriteLine("R3 value: {0}", R3);

            R4 = ExecuteFactor(FactorType.R4, defenderFactors, defenderFactors, defenderHero, defender, log);
            log.WriteLine("R4 value: {0}", R4);



            if ((attackType == AttackType.Ranged && attackerRef.RangePenalty == Penalty.value50) 
                || ((attackType == AttackType.Melee) 
                        && System.Array.Exists(attacker.AttackType, element => element == AttackType.Ranged)
                        && !attackerFactors.ContainsKey(FactorType.NoMeleePenalty))
                )
                R5 = (decimal)0.5;
            log.WriteLine("R5 (melee or ranged penalty) value: {0}", R5);

            if (attackType == AttackType.Ranged && attackerRef.ObstaclePenalty == Penalty.value50)
                R6 = (decimal)0.5;
            log.WriteLine("R6 (obstacle penalty) value: {0}", R6);

            R7 = ExecuteFactor(FactorType.R7, defenderFactors, defenderFactors, attackerHero, attacker, log);
            log.WriteLine("R7 value: {0}", R7);

            R8 = ExecuteFactor(FactorType.R8, defenderFactors, defenderFactors, defenderHero, defender, log);
            log.WriteLine("R8 value: {0}", R8);


            decimal damageMin =  DMGbmin * (1 + I1 + I2 + I3 + I4 + I5) * (1 - R1) * (1 - R2 - R3) * (1 - R4) * (1 - R5) * (1 - R6) * (1 - R7) * (1 - R8);
            decimal damageMax =  DMGbmax * (1 + I1 + I2 + I3 + I4 + I5) * (1 - R1) * (1 - R2 - R3) * (1 - R4) * (1 - R5) * (1 - R6) * (1 - R7) * (1 - R8);
            log.WriteLine("Damage min: {0}, Damage max: {1}", damageMin, damageMax);

            int damageMinInt = System.Convert.ToInt32(System.Math.Ceiling(damageMin));
            int damageMaxInt = System.Convert.ToInt32(System.Math.Ceiling(damageMax));
            
            // side effect of Armorer - if damage is not int and defender has armorer secondardy skill then damage is decreased by 1
            bool armorer = false;
            if (damageMin - damageMinInt == 0 
                || damageMax - damageMaxInt == 0)
            {
                int i=0;
                foreach (object refItem in defenderHero.Items)
                {
                    if (refItem is Reference && defenderHero.ItemsElementName[i].ToString() == "SecondarySkill")
                        if (((Reference)refItem).Name == "Armorer")
                            armorer = true;
                    ++i;
                }

                if (armorer && damageMin - damageMinInt == 0)
                {
                    damageMinInt --;
                    log.WriteLine("Side effect of armorer for min damage added");
                }
                if (armorer && damageMax - damageMaxInt == 0)
                {
                    damageMaxInt --;
                    log.WriteLine("Side effect of armorer for max damage added");                    
                }
            }

            FightResult result = new FightResult();

            result.DamageMin = damageMinInt;
            result.DamageMax = damageMaxInt;
            result.DamageAvg = (damageMinInt + damageMaxInt) / 2;


            result.KilledMin = System.Convert.ToInt32(System.Math.Floor((decimal)(result.DamageMin) / (decimal)defender.Health));
            result.KilledMax = System.Convert.ToInt32(System.Math.Floor((decimal)(result.DamageMax) / (decimal)defender.Health));
            result.KilledAvg = System.Convert.ToInt32(System.Math.Floor((decimal)(result.DamageAvg) / (decimal)defender.Health));

            log.WriteLine("Left {0} damage for {1} from previous rounds", damageLeft, defenderRef.Name);
            
            if (damageLeft == 0)
                damageLeft = defender.Health;
                
            if (result.KilledMin > defenderCount)
                result.KilledMin = defenderCount;
            else
            {
                result.DamageLeftMin = damageLeft - result.DamageMin % defender.Health;
                if (result.DamageLeftMin <= 0)
                {
                    if (result.DamageLeftMin < 0)
                        result.DamageLeftMin = defender.Health + result.DamageLeftMin;
                    ++result.KilledMin;
                }

            }

            if (result.KilledMax > defenderCount)
                result.KilledMax = defenderCount;
            else
            {
                result.DamageLeftMax = damageLeft - result.DamageMax % defender.Health;
                if (result.DamageLeftMax <= 0)
                {
                    if (result.DamageLeftMax < 0)
                        result.DamageLeftMax = defender.Health + result.DamageLeftMax;
                    ++result.KilledMax;
                }
            }

            if (result.KilledAvg > defenderCount)
                result.KilledAvg = defenderCount;
            else
            {
                result.DamageLeftAvg = damageLeft - result.DamageAvg % defender.Health;
                if (result.DamageLeftAvg <= 0)
                {
                    if (result.DamageLeftAvg < 0)
                        result.DamageLeftAvg = defender.Health + result.DamageLeftAvg;
                    ++result.KilledAvg;
                }
            }

            log.WriteLine("Damage min: {0} ronuded, Damage max: {1} rounded", damageMinInt, damageMaxInt);
            log.WriteLine("{0} killed min: {1}, max: {2} {3}", attacker.Name, result.KilledMin, result.KilledMax, defender.Name);


            if (attackType == AttackType.Ranged)
            {
                log.WriteLine("Ranged attack, no Retaliations");
                result.Retaliations = 0;
            }
            else
            {
                if (attackerFactors.ContainsKey(FactorType.NoEnemyRetaliation))
                {
                    log.WriteLine("{0} has no Retaliations ability", attackerRef.Name);
                    result.Retaliations = 0;
                }
                else 
                {
                    result.Retaliations = 1;
                    log.WriteLine("{0} relaties once", defenderRef.Name);
                }
            }

            if ( attackType == AttackType.Melee &&  attackerFactors.ContainsKey(FactorType.StrikesTwice))
            {
                log.WriteLine("{0} has double attack", attackerRef.Name);
                result.DoubleAttack = true;
            }
            if ( attackType == AttackType.Ranged &&  attackerFactors.ContainsKey(FactorType.ShootsTwice))
            {
                log.WriteLine("{0} has double attack", attackerRef.Name);
                result.DoubleAttack = true;
            }




            return result;
        }



    }

    
}