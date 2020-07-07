using Lycium.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UTProject.Model;
using Xunit;

namespace UTProject
{

    [Trait("映射测试", "普通类")]
    public class NormalMapperTest
    {
        [Fact(DisplayName = "字段映射类型转换测试")]
        public void Test1()
        {
            //MapperBuilder<NormalSrcFieldModel,NormalDstFieldModel>.
            NormalSrcFieldModel src = new NormalSrcFieldModel();
            var date = DateTime.Now;
            src.Date = date.ToString();
            var dst = LyciumMapper<NormalDstFieldModel>.MapperFrom(src);
            Assert.Equal(src.Age, dst.Age);
            Assert.Equal(date.Year, dst.Date.Year);
            Assert.Equal(date.Month, dst.Date.Month);
            Assert.Equal(date.Day, dst.Date.Day);
            Assert.Equal(date.Hour, dst.Date.Hour);
            Assert.Equal(date.Minute, dst.Date.Minute);
            Assert.Equal(date.Second, dst.Date.Second);
        }


        [Fact(DisplayName = "字段映射忽略大小写测试")]
        public void Test2()
        {
            MapperBuilder<NormalSrcFieldModel, NormalDstFieldModel>
                .Create()
                .IgnoreCase()
                .Compile();
            NormalSrcFieldModel src = new NormalSrcFieldModel();
            var date = DateTime.Now;
            src.Date = date.ToString();
            var dst = LyciumMapper<NormalDstFieldModel>.MapperFrom(src);
            Assert.Equal(src.Name, dst.name);
            Assert.Equal(src.Age, dst.Age);
            Assert.Equal(date.Year, dst.Date.Year);
            Assert.Equal(date.Month, dst.Date.Month);
            Assert.Equal(date.Day, dst.Date.Day);
            Assert.Equal(date.Hour, dst.Date.Hour);
            Assert.Equal(date.Minute, dst.Date.Minute);
            Assert.Equal(date.Second, dst.Date.Second);
        }


        [Fact(DisplayName = "字段映射前缀测试")]
        public void Test3()
        {

            MapperBuilder<NormalSrcFieldModel, NormalDstFieldModel>
                .Create()
                .SetPrefix("td_")
                .Compile();
            NormalSrcFieldModel src = new NormalSrcFieldModel();
            var dst = LyciumMapper<NormalDstFieldModel>.MapperFrom(src);
            Assert.Equal(src.ida.ToString(), dst.td_ida);
            
        }


    }

}
