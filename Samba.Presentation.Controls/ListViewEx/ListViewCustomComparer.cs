/*
 *   Demo program for ListView Sorting
 *   Created by Li Gao, 2007
 *   
 *   Modified for demo purposes only.
 *   This program is provided as is and no warranty or support.
 *   Use it as your own risk
 * */

using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;

namespace Samba.Presentation.Controls.ListViewEx
{
    public abstract class ListViewCustomComparer: IComparer
    {
        protected Dictionary<string, ListSortDirection> sortColumns = new Dictionary<string, ListSortDirection>();
        
        public void AddSort(string sortColumn, ListSortDirection dir)
        {
            if (sortColumns.ContainsKey(sortColumn))
                sortColumns.Remove(sortColumn);           
           
           sortColumns.Add(sortColumn, dir);
        }

        public void ClearSort()
        {
            sortColumns.Clear();
        }

        protected List<string> GetSortColumnList()
        {
            List<string> result = new List<string>();
            Stack<string> temp = new Stack<string>();

            foreach (string col in sortColumns.Keys)
            {
                temp.Push(col);
            }

            while (temp.Count > 0)
            {
                result.Add(temp.Pop());
            }

            return result;
        }
               
        public abstract int Compare(object x, object y);
    }
       
}
