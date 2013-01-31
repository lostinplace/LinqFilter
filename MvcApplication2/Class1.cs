using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;

namespace MvcApplication2
{
  public static class Class1
  {
    public static Database1Entities context = new Database1Entities();
    public static void test()
    {
      var x= context.table1.Filter<table1>(new { stringData="brian", intData= 521});
      int l = x.Count();


    }

    private static string[] names = new string[]{
      "chris",
      "brian",
      "alison",
      "bill",
      "sonya",
      "sophia",
      "olivia",
      "kristen",
      "jake",
      "lindsey",
      "aaron",
      "andrew",
      "sarah",
      "jason",
      "richard",
      "james",
      "daniel",
      "claudia",
      "howard",
      "jennifer",
      "lauren",
      "ishmael"
    };

    public static void seeddb()
    {
      table1 table;
      Random rnd = new Random();
      for (int i = 0; i < 10000; i++)
      {
        table = new table1();
        table.stringData=names[rnd.Next(names.Length)];
        table.intData = rnd.Next(999);
        table.datetimeData = DateTime.Now.AddHours(rnd.Next(10000) - 5000);
        context.table1.Add(table);
      }
      context.SaveChanges();
    }
  }

  public enum FilterTypes
  {
    /*
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
     */
    Equals
  }

  public static class Utilities
  {
    

    private static ConcurrentDictionary<Type, List<PropertyInfo>> cachedPropertyDict  = new ConcurrentDictionary<Type,List<PropertyInfo>>();

    public static IQueryable<T> Filter<T>(this IQueryable<T> aQuery, object aFilter, FilterTypes aFilterType=FilterTypes.Equals, bool ignoreNulls=false)
    {
      Type varType = typeof(T);
      if (!cachedPropertyDict.ContainsKey(typeof(T)))
        cachedPropertyDict.AddOrUpdate(varType, varType.GetProperties().ToList(),(x,y)=>y);
      
      foreach (PropertyInfo item in aFilter.GetType().GetProperties())
	    {
        if (!cachedPropertyDict[varType].Any(x => x.Name == item.Name)) continue;
        object val = item.GetValue(aFilter);
        if (val==null)continue;
          
        Type valType = val.GetType();
        Expression fVal = Expression.Constant(val);
        
        ParameterExpression lParam = Expression.Parameter(typeof(T), "lParam");
        Expression propRef = Expression.Property(lParam, item.Name);
        Expression cnvPropRef = Expression.Convert(propRef, valType);
        Expression eval = Expression.Equal(cnvPropRef, fVal);
        
        Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(eval, lParam);
        aQuery = aQuery.Where(lambda);
	    }

      return aQuery;
    }
  }
}