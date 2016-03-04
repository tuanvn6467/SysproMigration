namespace Migration
{
    public class Source
    {
        public string Top { get; set; }

        public string Tables { get; set; }

        public string Join { get; set; }

        public string Join1 { get; set; }

        public string Join2 { get; set; }

        public string Join3 { get; set; }

        public string Join4 { get; set; }

        public string Join5 { get; set; }

        public string Join6 { get; set; }

        public string Join7 { get; set; }

        public string Join8 { get; set; }

        public string Join9 { get; set; }

        public string Join10 { get; set; }

        public string JoinTotal
        {
            get
            {
                var jointotal = "";
                if (!string.IsNullOrEmpty(Join))
                {
                    jointotal += " \n" + Join;
                }
                if (!string.IsNullOrEmpty(Join1))
                {
                    jointotal += " \n" + Join1;
                }
                if (!string.IsNullOrEmpty(Join2))
                {
                    jointotal += " \n" + Join2;
                }
                if (!string.IsNullOrEmpty(Join3))
                {
                    jointotal += " \n" + Join3;
                }
                if (!string.IsNullOrEmpty(Join4))
                {
                    jointotal += " \n" + Join4;
                }
                if (!string.IsNullOrEmpty(Join5))
                {
                    jointotal += " \n" + Join5;
                }
                if (!string.IsNullOrEmpty(Join6))
                {
                    jointotal += " \n" + Join6;
                }
                if (!string.IsNullOrEmpty(Join7))
                {
                    jointotal += " \n" + Join7;
                }
                if (!string.IsNullOrEmpty(Join8))
                {
                    jointotal += " \n" + Join8;
                }
                if (!string.IsNullOrEmpty(Join9))
                {
                    jointotal += " \n" + Join9;
                }
                if (!string.IsNullOrEmpty(Join10))
                {
                    jointotal += " \n" + Join10;
                }
                return jointotal;
            }
        }

        public string JoinUser { get; set; }

        public string Where { get; set; }

        public string OrderBy { get; set; }

        public string Script { get; set; }

        public int TimeOut { get; set; }
    }
}
