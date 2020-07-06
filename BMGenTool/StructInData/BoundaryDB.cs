using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MetaFly.Datum.Figure;
using MetaFly.Serialization;

namespace BMGenTool.Info
{
    public class Line_boundary_BM_beacons
    {
        public List<BEACON> Beacon { get; set; }
        public class BEACON
        {
            [XmlElement]
            public StringData Id { get; set; }

            [XmlElement]
            public StringData Name { get; set; }

            [DefaultValue("0")]
            [XmlElement]
            public StringData Version { get; set; }

            [XmlElement]
            public StringData BMB_SDDB_distance { get; set; }
        }

    }

}
