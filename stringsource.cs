using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using Wv.Utils;
using Wv.Schedulator;

namespace Wv.Schedulator
{
    public class StringSource : Source
    {
	protected string[] lines;
	Log log;

	public StringSource(Schedulator s, string name, string[] lines)
	    : base(s, name)
	{
	    log = new Log(name);
	    this.lines = lines;
	    
	    // process "import" lines right away, to create additional
	    // sources.
	    foreach (string line in lines)
	    {
		string[] args = word_split(line.Trim());
		string cmd = wv.shift(ref args).ToLower();
		
		if (cmd == "import" || cmd == "plugin")
		{
		    log.log("Creating plugin from line: '{0}'", line);
		    if (args.Length < 2)
			err(0, "Not enough parameters to '{0}'", cmd);
		    else
		    {
			SourceRegistry reg = new SourceRegistry();
			reg.create(s, args[0], args[1]);
		    }
		}
	    }
	}
	
	public static Source create(Schedulator s, string name,
				    string prefix, string suffix)
	{
	    return new StringSource(s, name, suffix.Split('\n'));
	}
	
	static string[] get_file(string filename)
	{
	    try
	    {
		StreamReader r = File.OpenText(filename);
		return r.ReadToEnd().Split('\n');
	    }
	    catch (IOException)
	    {
		// nothing
	    }
	    return "".Split('\n');
	}
    
	public static Source create_from_file(Schedulator s, string name,
					      string prefix, string suffix)
	{
	    return new StringSource(s, name, get_file(suffix));
	}

	static public string[] word_split(string s)
	{
	    string bra = "\"'([{";
	    string ket = "\"')]}";

	    ArrayList list = new ArrayList();
	    Stack nest = new Stack();
	    string buf = ""; // even if it's empty, always add the first word
	    foreach (char c in s)
	    {
		int is_bra = bra.IndexOf(c);
		int is_ket = ket.IndexOf(c);

		if (nest.Count == 0)
		{
		    if (c == ' ' || c == '\t')
		    {
			if (buf != null)
			    list.Add(buf);
			buf = null;
		    }
		    else
			buf += c;
		}
		else
		{
		    buf += c;

		    if (is_ket >= 0 && (char)nest.Peek() == c)
		    {
			nest.Pop();
			continue;
		    }
		}

		if (is_bra >= 0)
		    nest.Push(ket[is_bra]);
	    }

	    // even if it's empty, always add the last word
	    list.Add(buf==null ? "" : buf);

	    string[] result = new string[list.Count];
	    int ii = 0;
	    foreach (string ss in list)
		result[ii++] = ss;
	    return result;
	}

	void err(int lineno, string fmt, params object[] args)
	{
	    log.log(lineno.ToString() + ": " + fmt, args);
	}

	static string dequote(string s, string bra, string ket)
	{
	    int idx_bra = bra.IndexOf(s[0]);
	    int idx_ket = ket.IndexOf(s[s.Length-1]);

	    if (idx_bra != -1 && idx_bra == idx_ket)
		return s.Substring(1, s.Length-2);
	    else
		return s;
	}

	static string dequote(string s, string bra)
	{
	    return dequote(s, bra, bra);
	}

	static string dequote(string s)
	{
	    return dequote(s, "\"'");
	}
	
	public static TimeSpan parse_estimate(int lineno, string s)
	{
	    Regex re = new Regex(@"([0-9]*(\.[0-9]*)?)\s*([a-zA-Z]*)");
	    Match match = re.Match(s);
	    GroupCollection grp = match.Groups;
	    if (!match.Success || grp.Count < 4)
	    {
		//err(lineno, "Can't parse estimate '{0}'", s);
		return TimeSpan.FromHours(0);
	    }

	    double num = wv.atod(grp[1].ToString());
	    string units = grp[3].ToString().ToLower() + " ";
	    switch (units[0])
	    {
	    case 'd':
		return TimeSpan.FromHours(num*8);
	    case 'h':
	    case ' ':
		return TimeSpan.FromHours(num);
	    case 'm':
		return TimeSpan.FromMinutes(num);
	    case 's':
		return TimeSpan.FromSeconds(num); // wow, you're fast!
	    }

	    //err(lineno, "Unknown unit '{0}' in '{1}'", units, s);
	    return TimeSpan.FromHours(0);
	}
	
	class DelayedTask
	{
	    public Source source;
	    public bool external;
	    public Task oldtask, task;
	    public Task parent;
	    public DelayedTask dtparent;

	    public int lineno;
	    public string name;

	    public FixFor fixfor;
	    public int priority;
	    
	    public DateTime donedate, startdate, duedate;

	    public TimeSpan currest = TimeSpan.MaxValue;
	    public TimeSpan elapsed = TimeSpan.MaxValue;
	    public bool done;
	    public DateSlider habits;
	    
	    public string make_id()
	    {
		// this needs to be "as unique as possible" given that
		// all these tasks come from a text file, and yet not
		// change when the text file changes.  It's impossible to
		// be perfect here, so we do what we can.
		if (dtparent != null)
		    return dtparent.make_id() + ":" + name;
		else
		    return name;
	    }
	    
	    public void apply_from(Task t)
	    {
		if (fixfor == null)
		    fixfor = t.fixfor;
		if (priority <= 0)
		    priority = t.priority;

		if (wv.isempty(donedate))
		    donedate = t.donedate;
		if (wv.isempty(startdate))
		    startdate = t.startdate;
		if (wv.isempty(duedate))
		    duedate = t.duedate;
		if (currest == TimeSpan.MaxValue)
		    currest = t.currest;
		if (elapsed == TimeSpan.MaxValue)
		    elapsed = t.elapsed;
		if (!done)
		    done = t.done;
		if (habits == null)
		    habits = t.habits;
	    }
	    
	    public void apply_to(Task t)
	    {
		task = t;
		if (dtparent != null && dtparent.task != null)
		    parent = dtparent.task;
		if (parent != null)
		    t.parent = parent;
		if (t.parent == t)
		    throw new ArgumentException("task is its own parent!");

		if (fixfor != null)
		    t.fixfor = fixfor;
		if (priority > 0)
		    t.priority = priority;

		if (!wv.isempty(donedate))
		    t.donedate = donedate;
		else if (wv.isempty(t.donedate))
		    t.donedate = source.s.align;
		if (!wv.isempty(startdate))
		    t.startdate = startdate;
		if (!wv.isempty(duedate))
		    t.duedate = duedate;
		if (currest != TimeSpan.MaxValue && wv.isempty(t.currest))
		    t.currest = currest;
		if (elapsed != TimeSpan.MaxValue && wv.isempty(t.elapsed))
		    t.elapsed = elapsed;
		if (done)
		    t.done = done;
		if (habits != null)
		    t.habits = habits;
	    }

	    public class IdCompare : IComparer
	    {
		public int Compare(object _x, object _y)
		{
		    DelayedTask x = (DelayedTask)_x;
		    DelayedTask y = (DelayedTask)_y;
		    
		    return x.lineno.CompareTo(y.lineno);
		}
	    }

	    // this is a simple ordering that makes parents come out
	    // before their children, so that we can be sure to fill in
	    // the parent's values before we fill in its children's values
	    // by copying the parent.
	    public class TopologicalCompare : IComparer
	    {
		public int Compare(object _x, object _y)
		{
		    DelayedTask x = (DelayedTask)_x;
		    DelayedTask y = (DelayedTask)_y;

		    if (x.dtparent == y.dtparent)
			return 0; // effectively equal, even if null
		    else if (x.dtparent == null)
			return Compare(x, y.dtparent);
		    else if (y.dtparent == null)
			return Compare(x.dtparent, y);
		    else if (x.dtparent != y.dtparent)
			return Compare(x.dtparent, y.dtparent);
		    else
			return x.lineno.CompareTo(y.lineno);
		}
	    }
	}

        class DtIndex
	{
	    public DelayedTask parent;
	    public string name;
	    
	    public DtIndex(DelayedTask parent, string name)
	    {
		this.parent = parent;
		this.name = name;
	    }
	    
	    public override bool Equals(object o)
	    {
		DtIndex y = o as DtIndex;
		if (y == null)
		    return false;
		else
		    return parent == y.parent && name == y.name;
	    }
	    
	    public override int GetHashCode()
	    {
		return name.GetHashCode();
	    }
	}

	ArrayList dtasks = new ArrayList();
	Hashtable created_tasks = new Hashtable();
	
	Task find_task(string title)
	{
	    foreach (Task t in s.tasks)
		if (t.name == title || t.moniker == title)
		    return t;
	    return null;
	}

	void parse_task(int level, string[] args,
			ArrayList parents, int lineno,
			ref FixFor last_fixfor, DateSlider habits,
			bool external)
	{
	    if (level > parents.Count)
		err(lineno, "Level-{0} bug with only {1} parents!",
		    level, parents.Count);
	    else if (level <= parents.Count)
		parents.RemoveRange(level, parents.Count - level);

	    int pri = -1;
	    DateTime startdate = DateTime.MinValue;
	    DateTime duedate = DateTime.MinValue;
	    DateTime donedate = DateTime.MinValue;
	    TimeSpan currest = TimeSpan.MaxValue;
	    TimeSpan elapsed = TimeSpan.MaxValue;
	    for (int i = 0; i < args.Length; i++)
	    {
		string a = args[i];
		if (a[0] == '[' && a[a.Length-1] == ']')
		{
		    a = dequote(wv.shift(ref args, i), "[", "]");
		    --i; // we just ate this array element!

		    string[] words = word_split(a);
		    for (int wi = 0; wi < words.Length; wi++)
		    {
			string word = words[wi].ToLower();
			if (word == "start")
			{
			    startdate = wv.date(words[wi+1]).Date;
			    wi++;
			}
			else if (word == "end" || word == "due")
			{
			    duedate = wv.date(words[wi+1]).Date;
			    wi++;
			}
			else if (word == "done" || word == "finished")
			{
			    donedate = wv.date(words[wi+1]).Date;
			    wi++;
			}
			else if (word[0] == 'p')
			    pri = Int32.Parse(word.Substring(1));
			else if (Char.IsDigit(word[0]))
			{
			    if (currest == TimeSpan.MaxValue)
				currest = parse_estimate(lineno, word);
			    else if (elapsed == TimeSpan.MaxValue)
				elapsed = parse_estimate(lineno, word);
			    else
				err(lineno, "Extra time '{0}'", word);
			}
			else
			    err(lineno, "Unknown flag '{0}' in '{1}'",
				word, a);
		    }
		}
	    }

	    DelayedTask parent = (parents.Count > 0
			   ? (DelayedTask)parents[parents.Count-1]
			   : null);
	    string title = String.Join(" ", args);
	    
	    DelayedTask d = new DelayedTask();
	    d.source = this;
	    d.lineno = lineno;
	    d.name = title;
	    d.external = external;

	    d.dtparent = parent;
	    d.fixfor = last_fixfor;
	    d.priority = pri;
	    d.donedate = donedate;
	    d.startdate = startdate;
	    d.duedate = duedate;
	    d.currest = currest;
	    d.elapsed = elapsed;
	    if (elapsed != TimeSpan.MaxValue && currest == elapsed)
		d.done = true;
	    d.habits = habits;

	    //log.log("Parent of '{0}' is '{1}'",
	    //	    d.name, d.dtparent==null ? "(none)" : d.dtparent.name);

	    parents.Add(d);
	    dtasks.Add(d);
	}

	public override void make_basic()
	{
	    int lineno = 0;
	    ArrayList parents = new ArrayList();
	    string projname = "";
	    FixFor last_fixfor = null;
	    DateSlider habits = null;

	    foreach (string str in lines)
	    {
		string[] args = word_split(str.Trim());
		string cmd = wv.shift(ref args).ToLower();

		++lineno;

		if (cmd == "" || cmd[0] == '#') // blank or comment
		    continue;
		
		switch (cmd)
		{
		case "import":
		case "plugin":
		    // already handled earlier
		    break;
		    
		case "milestone":
		case "release":
		case "version":
		    if (args.Length > 0)
		    {
			string fixforname = dequote(args[0]);
			int idx = fixforname.IndexOf(':');
			if (idx >= 0)
			{
			    projname = fixforname.Substring(0, idx);
			    fixforname = fixforname.Substring(idx+1);
			}
			last_fixfor = 
			    s.fixfors.Add(s.projects.Add(projname),
					  fixforname);
		    }
		    else
			err(lineno, "'{0}' requires an argument", cmd);
		    if (args.Length > 1)
			last_fixfor.add_release(wv.date(args[1]));

		    log.log("New milestone: {0}", last_fixfor.name);
		    break;

		case "bounce":
		    if (last_fixfor != null)
			foreach (string arg in args)
			    last_fixfor.add_release(wv.date(arg));
		    else
			err(lineno,
			    "Can't 'bounce' until we have a 'milestone'");
		    break;

		case "loadfactor":
		    if (habits == null)
			habits = s.default_habits;
		    habits = habits.new_loadfactor(wv.atod(args[0]));
		    break;

		case "workinghours":
		    if (habits == null)
			habits = s.default_habits;
		    if (args.Length < 7)
			err(lineno,
			    "'Workinghours' needs exactly 7 numbers");
		    else
		    {
			double[] hpd = new double[7];
			for (int i = 0; i < 7; i++)
			    hpd[i] = wv.atod(args[i]);
			habits = habits.new_hours_per_day(hpd);
		    }
		    break;

		case "alignday":
		    s.align = wv.date(args[0]);
		    break;

		case "today":
		    s.now = wv.date(args[0]);
		    break;

		case "*":
		case "**":
		case "***":
		case "****":
		case "*****":
		case "******":
		case "*******":
		case "********":
		case "*********":
		case "**********":
		    parse_task(cmd.Length-1, args,
			       parents, lineno, ref last_fixfor, habits,
			       false);
		    break;
		    
		case "!":
		    parse_task(parents.Count, args,
			       parents, lineno, ref last_fixfor, habits,
			       true);
		    break;

		default:
		    err(lineno, "Unknown command '{0}'!", cmd);
		    break;
		}
		
		if (habits != null)
		{
		    s.default_habits = habits;
		    if (last_fixfor != null)
		    {
			last_fixfor.default_habits = habits;
			last_fixfor.project.default_habits = habits;
		    }
		}
	    }
	}

	public override Task[] make_tasks()
	{
	    // go back to the original order before adding the tasks,
	    // so the list looks the way the user wanted it
	    dtasks.Sort(new DelayedTask.IdCompare());
	    
	    foreach (DelayedTask d in dtasks)
	    {
		if (!d.external)
		{
		    DtIndex idx = new DtIndex(d.dtparent, d.name);
		    
		    // prevent creation of duplicate tasks: simply modify
		    // the existing task instead
		    d.task = (Task)created_tasks[idx];
		    if (d.task == null)
		    {
			d.task = s.tasks.Add(this, d.make_id(), d.name);
			created_tasks.Add(idx, d.task);
		    }
		}
	    }
	    
	    return null;
	}

	public override void cleanup_tasks()
	{
	    foreach (DelayedTask d in dtasks)
	    {
		if (d.external)
		{
		    d.task = find_task(d.name);
		    if (d.task != null)
			d.apply_from(d.task);
		}
		if (d.task == null)
		    d.task = s.tasks.Add(this, d.make_id(), d.name);
	    }
	    
	    // let's fill any optional information from parents into
	    // children.  First, we'll want to sort so that parents
	    // always come before children, so we're guaranteed to have
	    // finished any given parent before we get to its child.
	    dtasks.Sort(new DelayedTask.TopologicalCompare());
	    
	    foreach (DelayedTask d in dtasks)
	    {
		DelayedTask p = d.dtparent;
		if (p != null)
		{
		    if (d.fixfor == null)
			d.fixfor = p.fixfor;
		    if (d.priority < 0)
			d.priority = p.priority;
		    if (d.habits == null)
			d.habits = p.habits;
		}
		d.apply_to(d.task);
	    }
	}
    }
}
