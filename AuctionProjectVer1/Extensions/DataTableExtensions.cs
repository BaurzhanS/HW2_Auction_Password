using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionProjectVer1.Extensions
{
    public static class DataTableExtensions
    {
        public static DataRow NewRowWithData<T>(
                this DataTable table, T item)
        {
            var newRow = table.NewRow();
            foreach (var propertyInfo in item.GetType()
                .GetProperties())
            {
                if(propertyInfo.GetValue(item)!= null)
                {
                    newRow[propertyInfo.Name] = propertyInfo.GetValue(item);
                }             
            }
            return newRow;
        }
    }
}
