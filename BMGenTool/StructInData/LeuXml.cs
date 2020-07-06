using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MetaFly.Datum.Figure;
using MetaFly.Serialization;
using MetaFly.Summer.Generic;

namespace BMGenTool.LEUXML
{
    public class LEU
    {
        public void updateGid(Generate.GID gid)
        {
            INPUT_BOARD.GID = new StringData(gid.ibGid);
            OUTPUT_BOARD.GID = new StringData(gid.ouGid);
            Encoder.NETWORK_GID = new StringData(gid.netGid);
        }

        [XmlAttribute]
        public StringData name { get; set; }

        [XmlElement]
        public input_board INPUT_BOARD { get; set; }
        public class input_board
        {
            [XmlAttribute]
            public StringData id { get; set; }

            [XmlElement]
            public StringData SLOT { get; set; }

            [XmlElement]
            public StringData GID { get; set; }

            [XmlElement]
            public StringData TYPE { get; set; }

        }

        [XmlElement]
        public output_board OUTPUT_BOARD { get; set; }
        public class output_board
        {
            [XmlElement]
            public StringData SLOT { get; set; }

            [XmlElement]
            public StringData GID { get; set; }

            [XmlElement]
            public StringData TYPE { get; set; }

            [XmlElement]
            public StringData TEL_number { get; set; }

        }

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

            public bool CheckAspect(ASPECT asp)
            {
                if (Aspect.Exists(x => x.Mask == asp.Mask))
                {
                    TraceMethod.RecordInfo($"Error: {asp.Mask} is repeat in balise {name}-{id}");
                    return false;
                }
                return true;
            }

            [XmlElement]
            public StringData Default_telegram { get; set; }

        }

        [XmlElement]
        public ENCODER Encoder { get; set; }
        public class ENCODER
        {
            [XmlAttribute]
            public StringData mode { get; set; }

            [XmlElement]
            public StringData TE_NUMBER { get; set; }

            [XmlElement]
            public StringData TEMPO_T1 { get; set; }

            [XmlElement]
            public StringData TEMPO_T2 { get; set; }

            [XmlElement]
            public StringData SYNC_MASTER_BOARD { get; set; }

            [XmlElement]
            public StringData NETWORK_GID { get; set; }

            [XmlElement]
            public StringData FIP_ENCODER_PN { get; set; }

            [XmlElement]
            public StringData FIP_SUB_NETWORK_ADD { get; set; }

            [XmlElement]
            public StringData SYSCKW_A { get; set; }

            [XmlElement]
            public StringData SYSCKW_B { get; set; }

            [XmlElement]
            public StringData SL_CHANGEOVER { get; set; }

        }

        [XmlElement]
        public NETWORK Network { get; set; }
        public class NETWORK
        {
            [XmlElement]
            public TRANSMIT_CHANNEL Transmit_Channel { get; set; }
            public class TRANSMIT_CHANNEL
            {
                [XmlElement]
                public StringData VFIP_ADD { get; set; }

                [XmlElement]
                public StringData Application_Category { get; set; }

                [XmlElement]
                public StringData FSFB2_Subnet_Ad { get; set; }

                [XmlElement]
                public StringData FSFB2_SRC_ADD { get; set; }

                [XmlElement]
                public StringData SID_A { get; set; }

                [XmlElement]
                public StringData SID_B { get; set; }

                [XmlElement]
                public StringData SINIT_A { get; set; }

                [XmlElement]
                public StringData SINIT_B { get; set; }

                [XmlElement]
                public dataver DATAVER { get; set; }
                public class dataver
                {
                    [XmlAttribute]
                    public StringData id { get; set; }

                    [XmlElement]
                    public StringData DATAVER_A { get; set; }

                    [XmlElement]
                    public StringData DATAVER_B { get; set; }

                    [XmlElement]
                    public StringData NUM_DATAVER { get; set; }

                    [XmlElement]
                    public StringData FSFB2_SRC_ADD { get; set; }
                }
            }

        }

        [XmlElement]
        public StringData POSIXDATE_TPC { get; set; }

        [XmlElement]
        public StringData VERID_TPC { get; set; }

        [XmlElement]
        public StringData POSIXDATE_TSE { get; set; }

        [XmlElement]
        public StringData VERID_TSE { get; set; }

        [XmlElement]
        public StringData POSIXDATE_TE { get; set; }

        [XmlElement]
        public StringData VERID_TE { get; set; }

    }

}
