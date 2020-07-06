using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MetaFly.Datum.Figure;
using MetaFly.Serialization;

namespace BMGenTool.Info
{
    public class Balise
    {
        public Balise()
        {
            Telegram = new TELEGRAM();
        }
        [XmlAttribute]
        public StringData name { get; set; }

        [XmlElement]
        public TELEGRAM Telegram { get; set; }
        public class TELEGRAM
        {
            [XmlValue]
            public StringData Value { get; set; }
            [XmlAttribute]
            public StringData type { get; set; }
        }

    }

}
