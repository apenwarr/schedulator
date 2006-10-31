#include "wvtest.cs.h"

using System;
using NUnit.Framework;
using Wv.Test;
using Wv.Utils;
using Wv.Schedulator;

[TestFixture]
public class DateSliderTests
{
    Log log = new Log("log");
    
    static DateTime date(string s)
    {
	return DateTime.Parse(s);
    }
    
    [Test] public void dateslider_test()
    {
	WVPASSEQ((int)DayOfWeek.Sunday, 0);
	WVPASSEQ((int)DayOfWeek.Monday, 1);
	
	DateSlider x = new DateSlider();
	double[] times = {6,5,5,5,5,5,9};
	x = x.new_hours_per_day(times);
	DateSlider y = (DateSlider)x.Clone();
	x.hours_per_day[3] = 77;
	times[3] = 99;
	WVPASSEQ(y.hours_per_day[6], 9);
	WVPASSEQ(y.hours_per_day[3], 5);
	WVPASSEQ(x.hours_per_day[3], 77);
	WVPASSEQ(times[3], 99);
    }
    
    [Test] public void dateslider_add_test()
    {
	DateTime point = date("2006-10-10 00:00:00");
	DateSlider slider = new DateSlider();
	
	// week multiples
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(40)),
		 date("2006-10-17 00:00:00"));
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(-40)),
		 date("2006-10-03 00:00:00"));
	slider = slider.new_loadfactor(2);
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(40)),
		 date("2006-10-24 00:00:00"));
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(-40)),
		 date("2006-09-26 00:00:00"));
	slider = slider.new_loadfactor(1);
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(80)),
		 date("2006-10-24 00:00:00"));
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(-80)),
		 date("2006-09-26 00:00:00"));
	
	// day multiples.  2006-10-10 is Tuesday == offset 2.
	double[] a = {0,1,2,3,4,5,6}; // 21 hours per week
	slider = slider.new_hours_per_day(a);
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(5)),
		 date("2006-10-12 00:00:00"));
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(-7)),
		 date("2006-10-07 00:00:00"));
	slider = slider.new_loadfactor(2);
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(2.5)),
		 date("2006-10-12 00:00:00"));
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(-3.5)),
		 date("2006-10-07 00:00:00"));
	slider = slider.new_loadfactor(1);
	
	// fractional days
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(2*21 + 4)),
		 date("2006-10-25 16:00:00"));
	WVPASSEQ(slider.add(point, TimeSpan.FromHours(-2*21 - 4)),
		 date("2006-09-23 12:00:00"));
    }
    
    [Test] public void dateslider_add_test2()
    {
	DateSlider slider = new DateSlider();
	DateTime dt1 = date("1997-01-29").AddHours(0.25*24);
	DateTime dt2 = date("1997-01-29").AddHours(0.75*24);
	DateTime odt1 = slider.add(dt1, TimeSpan.FromHours(4));
	DateTime odt2 = slider.add(dt2, TimeSpan.FromHours(-4));
	log.log("Start: {0} - {1}", dt1, dt2);
	WVPASSEQ(dt2.ToString(), odt1.ToString());
	WVPASSEQ(dt1.ToString(), odt2.ToString());
    }
    
    [Test] public void dateslider_add_test3()
    {
	double[] hpd = {8,8,8,8,8,8,8};
	DateSlider slider = new DateSlider().new_hours_per_day(hpd);
	DateTime point = date("2006-10-10 00:00:00");
	
	point = slider.add(point, -TimeSpan.FromHours(4));
	WVPASSEQ(point, date("2006-10-09 12:00:00"));
	point = slider.add(point, -TimeSpan.FromHours(3));
	WVPASSEQ(point, date("2006-10-09 03:00:00"));
	point = slider.add(point, -TimeSpan.FromHours(2));
	WVPASSEQ(point, date("2006-10-08 21:00:00"));
	point = slider.add(point, -TimeSpan.FromHours(1));
	WVPASSEQ(point, date("2006-10-08 18:00:00"));
    }
    
    [Test] public void dateslider_diff_test()
    {
	double[] hpd = {0,1,2,3,4,5,6}; // 21 hours per week
	DateSlider slider 
	    = (new DateSlider()).new_hours_per_day(hpd).new_loadfactor(0.5);
	DateTime point = date("2006-10-10 00:00:00");
	
	// week multiples
	WVPASSEQ(slider.diff(date("2006-10-17"), point),
		 TimeSpan.FromHours(42));
	WVPASSEQ(slider.diff(date("2006-10-24"), point),
		 TimeSpan.FromHours(2*42));
	
	// day multiples.  2006-10-10 is a Tuesday.
	WVPASSEQ(slider.diff(date("2006-10-12"), point),
		 TimeSpan.FromHours(10));
	WVPASSEQ(slider.diff(date("2006-10-16"), point),
		 TimeSpan.FromHours(40));
	
	// fractional days
	WVPASSEQ(slider.diff(date("2006-10-12 12:00:00"), point),
		 TimeSpan.FromHours(14));
	WVPASSEQ(slider.diff(date("2006-10-13 14:24:00"), point),
		 TimeSpan.FromHours(24));
	
	// fractional day landing on a zero-day, so result has to be
	// inaccurate
	WVPASSEQ(slider.diff(date("2006-10-15 16:00:00"), point),
		 TimeSpan.FromHours(40));
    }
}

