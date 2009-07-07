using System;
using System.Linq;
using Wv;
using Wv.Query;

namespace Whatever
{
    class Testy
    {
	[PrimaryKeyColumn] public int id;
	[Column] public string name;
	[Column] public int age;
	[Column] public DateTime birthdate;
    }
    
    class MyDb : Database
    {
        public MyDb(WvDbi dbi) : base(dbi) { }
        public MyDb(string dbi_moniker) : base(dbi_moniker) { }
	
	public QuerySet<Testy> Testy {
	    get { return new QuerySet<Testy>(dbi); }
	}
    }
    
    public static class WrapTest
    {
	public static void Main()
	{
	    var db = new MyDb("sqlite:foo.db");

	    foreach (Testy t in db.Testy.filter("name", "Jana"))
		wv.print("X: '{0};{1};{2};{3}'\n", 
			 t.id, t.name, t.age, t.birthdate);

	    var res =
		from r in db.Testy
		where r.name == "Avery"
		select r;
	    foreach (Testy t in res)
		wv.print("X: '{0};{1};{2};{3}'\n", 
			 t.id, t.name, t.age, t.birthdate);
	}
    }
}
