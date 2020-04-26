using System.IO;
using System.Data;
using System.Collections.Generic;

namespace Plazacraft.HOMM3.DamageSymulator.Core
{

    public class World
    {
        public class ParamInfo
        {
            public string ParamName
            {get; set;}

            public string AbilityName
            {get; set;}

            public string CreatureName
            {get; set;}
        }

        public Definition WorldDefinition
        {
            get { return _definition; }
        }


        public World(TextReader definitionReader)
        {
            _definition = Helper.DeserializeXML<Definition>(definitionReader);
            LoadDefinition(_definition);

        }
        public World(string definition)
        {
            _definition = Helper.DeserializeXML<Definition>(definition);
            LoadDefinition(_definition);
        }

        public World(Definition definition)
        {
            _definition = definition;
            LoadDefinition(_definition);
        }

        public Dictionary<string, Creature> Creatures
        { get { return _creatures;} }

        public Dictionary<string, CreatureAbility> CreatureAbilities
        { get { return _creatureAbilities; } }

        public Dictionary<string, HeroAbility> HeroAbilities
        { get { return _heroAbilities; } }

        public Dictionary<string, Spell> Spells
        { get { return _spells; } }

        public Dictionary<string, SecondarySkill> SecondarySkills
        { get { return _secondarySkills; } }

        public Dictionary<string, object> Modificators
        { get { return _modificators; } }


        public List<ParamInfo> Params
        {get {return _params; } }        


        private Definition _definition = null;
        private Dictionary<string, Creature> _creatures = new Dictionary<string, Creature>();
        private Dictionary<string, CreatureAbility> _creatureAbilities = new Dictionary<string, CreatureAbility>();
        private Dictionary<string, Spell> _spells = new Dictionary<string, Spell>();
        private Dictionary<string, object> _modificators = new Dictionary<string, object>();
        private Dictionary<string, SecondarySkill> _secondarySkills = new Dictionary<string, SecondarySkill>();
        private Dictionary<string, HeroAbility> _heroAbilities = new Dictionary<string, HeroAbility>();
        private List<ParamInfo> _params = new List<ParamInfo>();

        private void LoadDefinition(Definition definition)
        {
            if (definition.CreatureAbilities != null)
                foreach(CreatureAbility item in definition.CreatureAbilities.Items)
                {
                    _creatureAbilities.Add(item.Name, item);
                }
            if (definition.Creatures != null)
                foreach(Creature item in definition.Creatures.Items)
                {
                    _creatures.Add(item.Name, item);
                    if (item.Items != null)
                    {
                        foreach (Reference refItem in item.Items)
                        {
                            string paramsString = CreatureAbilities[refItem.Name].Params;
                            if (!string.IsNullOrEmpty(paramsString))
                            {
                                string [] split = paramsString.Split(' ');
                                foreach(string splitParam in split)
                                {
                                    _params.Add(new ParamInfo()
                                        {
                                            ParamName = splitParam
                                            ,AbilityName = refItem.Name
                                            ,CreatureName = item.Name
                                        }
                                    );
                                }
                            }
                        }
                    }
                }
            if (definition.Spells != null)
                foreach(Spell item in definition.Spells.Items)
                {
                    //_spells.Add(item.Name, item);
                }
            if (definition.Modificators != null)
                foreach(object item in definition.Modificators.Items)
                {
                    if (item is Modificator)
                        _modificators.Add(((Modificator)item).Name, item);
                    else if (item is LevelModificator)
                        _modificators.Add(((LevelModificator)item).Name, item);
                }
            if (definition.SecondarySkills != null)
                foreach(SecondarySkill item in definition.SecondarySkills.Items)
                {
                    _secondarySkills.Add(item.Name, item);
                }
            if (definition.HeroAbilities != null)
                foreach(HeroAbility item in definition.HeroAbilities.Items)
                {
                    _heroAbilities.Add(item.Name, item);
                }
        }


    }
}