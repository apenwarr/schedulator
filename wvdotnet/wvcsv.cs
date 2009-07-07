/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.Collections;
using System.Globalization;

namespace Wv
{
    public class WvCsv
    {
        string astext;
        ArrayList asarray;
        int pos = 0;
        
        public WvCsv(string toparse)
        {
            astext = toparse;
        }
        
        public WvCsv(ArrayList tounparse)
        {
            asarray = tounparse;
        }
        
        public bool hasMore()
        {
            return (pos < astext.Length);
        }
        
        //return first line of the ArrayList (or the ArrayList) as a string
        public string GetCsvLine()
        {
            return "";
        }
        
        //return the full ArrayList (multiple lines) as CSV
        public string GetCsvText()
        {
            return "";
        }
        
        //returns the next line parsed into an ArrayList
        public ArrayList GetLine()
        {
            string field = "";
            asarray = new ArrayList();
            
            while (pos < astext.Length)
            {
                if (astext[pos] == '\n')
                {
                    asarray.Add(null);
                    pos++;
                    return asarray;
                }
                
                //certainly a string                
                if (astext[pos] == '"')
                {
		    char lastChar = '"';
                    pos++;
                    while (pos < astext.Length)
                    {
                        if ((lastChar=='"') && ((astext[pos]==',') || 
                                                (astext[pos]=='\n')) )
                        {
                            if (field.EndsWith("\""))
                            {
                                string tmp = field.Substring(0,field.Length-1);
                                string temp = tmp.Replace("\"\"","");
                                if (((tmp.Length - temp.Length) %2 == 0) && 
                                    (!temp.EndsWith("\"")))
                                {
                                    field = tmp;
                                    break;
                                }
                            }
                        }
                             
                        field += astext[pos];
                        lastChar = astext[pos];
                        pos++;
                    }
                    
                    if ((pos==astext.Length) && (astext[pos-1]!='\n') && 
                                                field.EndsWith("\""))
                        field = field.Substring(0,field.Length-1);
                    
                    asarray.Add(field.Replace("\"\"","\""));
                }
                else
                {
                    while ((pos < astext.Length) && (astext[pos]!=',') && 
                                                    (astext[pos]!='\n'))
                    {
                        field += astext[pos];
                        pos++;
                    }

                    if (String.IsNullOrEmpty(field))
                        asarray.Add(null);
                    else
                        asarray.Add(field.Replace("\"\"","\""));

                }
                if ((pos < astext.Length) && (astext[pos]=='\n'))
                {
                    pos++;
                    return asarray;
                }
                    
                field = "";
                pos++;
            }

            
            return asarray;
        }
    }
} //namespace
