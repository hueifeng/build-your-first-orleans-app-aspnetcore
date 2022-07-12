using System.Text;
using OrleansURLShortener;

Dictionary<string, long> list = new Dictionary<string, long>();

for (long i = 0; i < 10000000000; i++)
{
    //var hash = ShortUrlGenerator.MurmurHash3(i + "test"+Random.Shared.Next(10000));
    //var code = ShortUrlGenerator.Generator(hash);


    ////var code = ShortUrlGenerator.Generator(hash);
    ////var code = GetCode(hash);
    //list.Add(Math.Abs(hash).ToString(), i);

    //byte[] srcBytes = Encoding.UTF8.GetBytes(i+"t1sa");
    //// HashSizeInBits=32 or 128
    //var cfg = new MurmurHash3Config() { HashSizeInBits = 32, Seed = 0 };
    //var mur = MurmurHash3Factory.Instance.Create(cfg);
    //var hv = mur.ComputeHash(srcBytes);
    //var base64 = hv.AsBase64String();
    //var hashBytes = hv.Hash;
    //var code = ShortUrlGenerator.Generator(i);
    //list.Add(code, i);


    var bytes = Encoding.UTF8.GetBytes(i + "test"+Random.Shared.Next(10000));
    var hashConfig = new System.Data.HashFunction.MurmurHash.MurmurHash3Config();
    hashConfig.Seed = 0;
    hashConfig.HashSizeInBits = 128;
    var murmurHash3 = System.Data.HashFunction.MurmurHash.MurmurHash3Factory.Instance.Create(hashConfig);
    var hv = murmurHash3.ComputeHash(bytes);
    var base64 = hv.AsBase64String();
    var hashBytes = hv.Hash;
    //var code = ShortUrlGenerator.Generator(i);
    //list.Add(code, i);
}

//Parallel.For(0, 10000000000, (i) => 
//{
//    var hash = ShortUrlGenerator.MurmurHash3(i + "test");
//    var code = ShortUrlGenerator.Generator(hash);
//    //var code = GetCode(hash);
//    list.Add(code,i);
//});


//var hash = ShortUrlGenerator.MurmurHash3("111121233121");
//var code1 = GetCode(hash);
//var code = ShortUrlGenerator.Generator(hash);

//var hash = ShortUrlGenerator.MurmurHash3("test");
var isRepeat = list.GroupBy(i => i).Where(g => g.Count() > 1);
var count = isRepeat.Count();
Console.ReadLine();
