using Lycium.Mapper;
using System;


public static class LyciumMapper<TDst>
{

    public static TDst MapperFrom<TSrc>(TSrc src)
    {
        return LyciumMapper<TSrc, TDst>.Mapper(src);
    }

}

public static class LyciumMapper<TSrc, TDst>
{
    public static Func<TSrc, TDst> Mapper;
    static LyciumMapper()
    {
        Mapper = MapperBuilder<TSrc, TDst>.Create().Compile();
    }
}

