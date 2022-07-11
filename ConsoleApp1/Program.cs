using OrleansURLShortener;

Dictionary<string,long> list = new Dictionary<string, long>();

for (long i = 0; i < 10000000000; i++)
{
    var hash = ShortUrlGenerator.MurmurHash3(i + "test");
    var code = ShortUrlGenerator.Generator(hash);
    //var code = GetCode(hash);
    list.Add(code, i);
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
