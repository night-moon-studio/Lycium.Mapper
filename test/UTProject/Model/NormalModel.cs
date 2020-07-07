using System;
using System.Collections.Generic;
using System.Text;

namespace UTProject.Model
{
    public class NormalSrcFieldModel
    {
        public NormalSrcFieldModel()
        {
            Name = "string";
            Age = 32;
            Date = DateTime.Now.ToString();
            ida = 321;
            src_id = 322;
            src_flags = 323;
        }
        public string Name;
        public int Age;
        public string Date;
        public long ida;
        public long src_id;
        public short src_flags;
    }

    public class NormalDstFieldModel 
    {

        public string name;
        public long Age;
        public DateTime Date;
        public string td_ida;
        public string id;
        public double td_flags;

    }

}
