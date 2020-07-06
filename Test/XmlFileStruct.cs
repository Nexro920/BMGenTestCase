using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MetaFly.Datum.Figure;
using MetaFly.Serialization;
using MetaFly.Summer.Generic;
using System.Diagnostics;

namespace BMGenTool.TestCase
{
    public class Balise
    {
        public static bool checkBaliseXmlInfo(string xml, string rightxml)
        {
            Balise leu = FileLoader.Load<Balise>(xml);
            Balise rightleu = FileLoader.Load<Balise>(rightxml);

            Debug.Assert(leu.name == rightleu.name);
            Debug.Assert(leu.Telegram == rightleu.Telegram);
            return true;
        }
        [XmlAttribute]
        public StringData name { get; set; }
        
        [XmlAttribute]
        public StringData Telegram { get; set; }

    }

    public class LEU
    {
        public static bool checkLEUXmlInfo(string leuxml, string rightleuxml)
        {
            TestCase.LEU leu = FileLoader.Load<TestCase.LEU>(leuxml);
            TestCase.LEU rightleu = FileLoader.Load<TestCase.LEU>(rightleuxml);

            Debug.Assert(leu.name == rightleu.name);
            Debug.Assert(leu.Output_balise.Count == rightleu.Output_balise.Count);

            for (int i = 0; i < leu.Output_balise.Count; ++i)
            {
                Debug.Assert(leu.Output_balise[i].id == rightleu.Output_balise[i].id);
                Debug.Assert(leu.Output_balise[i].Default_telegram == rightleu.Output_balise[i].Default_telegram);

                Debug.Assert(leu.Output_balise[i].Input.Count == rightleu.Output_balise[i].Input.Count);
                for (int j = 0; j < leu.Output_balise[i].Input.Count; j++)
                {
                    Debug.Assert(leu.Output_balise[i].Input[j].Channel == rightleu.Output_balise[i].Input[j].Channel);
                    Debug.Assert(leu.Output_balise[i].Input[j].index == rightleu.Output_balise[i].Input[j].index);
                }

                Debug.Assert(leu.Output_balise[i].Aspect.Count == rightleu.Output_balise[i].Aspect.Count);
                for (int k = 0; k < leu.Output_balise[i].Aspect.Count; k++)
                {
                    Debug.Assert(leu.Output_balise[i].Aspect[k].Mask == rightleu.Output_balise[i].Aspect[k].Mask);
                    Debug.Assert(leu.Output_balise[i].Aspect[k].Telegram == rightleu.Output_balise[i].Aspect[k].Telegram);
                }
            }
            return true;
        }

        [XmlAttribute]
        public StringData name { get; set; }
        
        [XmlElement]
        public List<OUTPUT_BALISE> Output_balise { get; set; }
        public class OUTPUT_BALISE
        {
            string name = "";
            public OUTPUT_BALISE()
            { }

            public OUTPUT_BALISE(string NAME, int ID)
            {
                name = NAME;
                id = new StringData(ID.ToString());
                telegram = new StringData("LONG");

                Input = new List<INPUT>();
                Aspect = new List<ASPECT>();
            }

            [XmlAttribute]
            public StringData id { get; set; }

            [XmlAttribute]
            public StringData telegram { get; set; }

            [XmlElement]
            public List<INPUT> Input { get; set; }
            public class INPUT
            {
                public int index = 0;
                [XmlElement]
                public StringData Channel { get; set; }

                [XmlElement]
                public StringData Number { get; set; }
            }

            [XmlElement]
            public List<ASPECT> Aspect { get; set; }
            public class ASPECT
            {
                [XmlElement]
                public StringData Mask { get; set; }

                [XmlElement]
                public StringData Telegram { get; set; }

            }

            [XmlElement]
            public StringData Default_telegram { get; set; }

        }

    }

}
