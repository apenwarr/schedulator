using System;
using Wv;

namespace Wv.Schedulator
{
    public class DateSlider : ICloneable
    {
	double _hours_per_week = 40;
	public double hours_per_week
	{
	    get { return _hours_per_week; }
	}
	
	// Sunday first, to match the enum DayOfWeek
	double[] _hours_per_day = {0,8,8,8,8,8,0};
	public double[] hours_per_day
	{
	    get { return _hours_per_day; }
	}
	
	double _loadfactor = 1.0;
	public double loadfactor
	{
	    get { return _loadfactor; }
	}
	
	
	public DateSlider new_hours_per_day(double[] hours_per_day)
	{
	    DateSlider d = (DateSlider)this.Clone();
	    
	    d._hours_per_day = (double[])hours_per_day.Clone();
	    d._hours_per_week = 0;
	    for (int i = 0; i < 7; i++)
	    {
		if (d._hours_per_day[i] < 0 || d._hours_per_day[i] > 24)
		    throw new ArgumentException("Each day can have 0 "
						+ "to 24 hours.");
		d._hours_per_week += d._hours_per_day[i];
	    }
	    if (d._hours_per_week < 1)
		throw new ArgumentException("A week needs "
					    + "at least 1 hour.");
	    return d;
	}
	
	public DateSlider new_loadfactor(double loadfactor)
	{
	    DateSlider d = (DateSlider)this.Clone();
	    d._loadfactor = loadfactor;
	    return d;
	}
	
	public DateSlider()
	{
	    // double[] x = {0,8,8,8,8,8,0};
	    // _hours_per_day = x;
	}
	
	public object Clone()
	{
	    DateSlider d = (DateSlider)this.MemberwiseClone();
	    d._hours_per_day = (double[])this.hours_per_day.Clone();
	    return d;
	}
	
	public override string ToString()
	{
	    return String.Format("(loadfactor={0:f2} "
				 + "hours=[{1} {2} {3} {4} {5} {6} {7}])",
				 loadfactor,
				 hours_per_day[0],
				 hours_per_day[1],
				 hours_per_day[2],
				 hours_per_day[3],
				 hours_per_day[4],
				 hours_per_day[5],
				 hours_per_day[6]);
	}
	
	public override bool Equals(object _y)
	{
	    DateSlider y = (DateSlider)_y;
	    return hours_per_day == y.hours_per_day
		&& loadfactor == y.loadfactor;
	}
	
	public override int GetHashCode()
	{
	    return hours_per_day.GetHashCode() + loadfactor.GetHashCode();
	}
	
	public DateTime add(DateTime point, TimeSpan span)
	{
	    WvLog log = new WvLog("slider", WvLog.L.Debug5);
	    log.print("* {0} + {1}", point, span.TotalHours);
	    
	    int sign = (span.Ticks < 0) ? -1 : 1;
	    bool less_one = false;
	    
	    // speed through times >= 1 week
	    double hpw = hours_per_week / loadfactor;
	    while (Math.Abs(span.TotalHours) >= hpw)
	    {
		log.print("W {0} + {1}      ({2})", point, span.TotalHours, hpw);
		point = point.AddDays(7*sign);
		span = span.Add(TimeSpan.FromHours(-hpw*sign));
	    }
	    
	    // inside a week, count a day at a time
	    while (Math.Abs(span.TotalHours) > 0.01)
	    {
		int day = (int)point.DayOfWeek;
		
		// we might be partway through the current day...
		double dayfraction = point.TimeOfDay.TotalHours / 24.0;
		if (sign > 0)
		{
		    if (dayfraction > 0.999)
		    {
			day = (day + 1) % 7;
			dayfraction = 0.0;
		    }
		    
		    // we want the remaining part of the day
		    dayfraction = 1.0 - dayfraction;
		}
		else
		{
		    if (dayfraction < 0.001)
		    {
			day--;
			if (day < 0) day = 6;
			dayfraction = 1.0;
			less_one = true;
		    }
		    
		    // we want the expired part of the day: keep dayfraction
		}
		
		double hpd = hours_per_day[day] * dayfraction / loadfactor;
		
		log.print("D {0} + {1}      ({2})", point, span.TotalHours, hpd);
		if (Math.Abs(span.TotalHours) >= hpd)
		{
		    // avoid rounding errors by forcing the date
		    if (sign > 0 || less_one)
			point = point.Date.AddDays(1*sign);
		    else
			point = point.Date;
		    span = span.Add(TimeSpan.FromHours(-hpd*sign));
		}
		else
		{
		    point = point.AddHours(span.TotalHours/hpd 
					     * 24 * dayfraction);
		    span = TimeSpan.Zero;
		}
	    }
	    
	    log.print(": {0} + {1}", point, span.TotalHours);
	    
	    // ah, rounding errors.  The above should make things work out
	    // to within the nearest minute or so, which is close enough(tm).
	    double th = point.TimeOfDay.TotalHours;
	    point = point.AddHours(Math.Round(th, 1) - th + 0.5/60.0/60.0);
	    point = new DateTime(point.Year, point.Month, point.Day,
				 point.Hour, point.Minute, 0, 0);
	    log.print(". {0} + {1}    ({2})",
		    point, span.TotalHours, Math.Round(th, 1));
	    return point;
	}
	
	// returns d = x - y, such that x = add(y, d).
	// Note that if day 'x' has zero working hours, this doesn't make
	// sense, so we choose the closest timespan that will get us up
	// to, but not including, x.
	public TimeSpan diff(DateTime x, DateTime y)
	{
	    WvLog log = new WvLog("slider", WvLog.L.Debug5);
	    wv.assert(x > y);
	    
	    DateTime point = y, end = x;
	    TimeSpan result = TimeSpan.Zero;
	    
	    while (point < end)
	    {
		TimeSpan remain = end - point;
		double hpd = hours_per_day[(int)point.DayOfWeek];
		
		if (remain.TotalDays >= 7)
		{
		    int weeks = (int)Math.Round(remain.TotalDays / 7);
		    point += TimeSpan.FromDays(weeks * 7);
		    result += TimeSpan.FromHours(
			 weeks * hours_per_week / loadfactor);
		}
		else if (remain.TotalHours >= 24)
		{
		    point += TimeSpan.FromDays(1);
		    result += TimeSpan.FromHours(hpd / loadfactor);
		}
		else if (remain.TotalHours > 0)
		{
		    double dayfraction = remain.TotalHours / 24;
		    point = end;
		    result += TimeSpan.FromHours(
			 dayfraction * hpd / loadfactor);
		}
	    }
	    
	    log.print("result: {0} hours", result.TotalHours);
	    return result;
	}
    }
}
