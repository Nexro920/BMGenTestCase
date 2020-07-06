using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MetaFly.Datum.Figure;
using MetaFly.Serialization;

namespace BMGenTool.Info
{
    public class LEU_filtered_values
    {
        [XmlAttribute]
        public StringData LINE_ID { get; set; }

        public List<leu> LEU { get; set; }
        public class leu
        {
            [XmlAttribute]
            public StringData ID { get; set; }

            [XmlAttribute]
            public StringData NAME { get; set; }

            public List<BEACON> Beacon { get; set; }

            public List<BEACON> beaconList
            {
                get
                { return Beacon; }
            }

            public class BEACON
            {
                public List<MESSAGE> msgList { get { return Message; } }
                public StringData outNum { get { return NUM; } }
                
                [XmlAttribute]
                public StringData ID { get; set; }

                [XmlAttribute]
                public StringData NAME { get; set; }

                [XmlAttribute]
                public StringData TYPE { get; set; }

                [XmlAttribute]
                public StringData NUM { get; set; }

                [XmlAttribute]
                public StringData VERSION { get; set; }

                [XmlAttribute]
                public StringData LINKED_SIGNAL { get; set; }

                [XmlElement]
                public StringData Variants_inputs { get; set; }

                public List<MESSAGE> Message { get; set; }
                public class MESSAGE
                {
                    
                    [XmlAttribute]
                    public StringData RANK { get; set; }

                    [XmlElement]
                    public StringData Variant_state { get; set; }
                    public string VarState { get { return Variant_state.ToString(); } }

                    [XmlElement]
                    public StringData Interoperable { get; set; }

                    public COMBINED_SECTIONS Combined_sections { get; set; }
                    public class COMBINED_SECTIONS
                    {
                        [XmlElement]
                        public StringData Reopening_section { get; set; }

                        [XmlElement]
                        public StringData Approach_section { get; set; }

                        [XmlElement]
                        public StringData Upstream_section { get; set; }

                        [XmlElement]
                        public StringData Overlap_section { get; set; }

                    }

                }

            }

        }

    }

}
