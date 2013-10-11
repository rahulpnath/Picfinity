using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Picfinity.DataModel
{
    public class PagedSourceLoader<T,K> : IPagedSource<T,K>
        where T:class 
    {
        private Func<T, IPagedResponse<K>> getPagedResponse;
        public PagedSourceLoader(Func<T, IPagedResponse<K>> GetPagedResponse)
        {
            getPagedResponse = GetPagedResponse;
        }

        #region IPagedSource

        public async Task<IPagedResponse<K>> GetPage(string query, int pageIndex, int pageSize)
        {
            try
            {
                if (AppSettings.HasInternetConnectivity)
                {
                    query += "&page=" + pageIndex;
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response = await client.GetAsync(query);
                    var data = await response.Content.ReadAsStreamAsync();
                    DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(T));
                    T dat = json.ReadObject(data) as T;
                    return getPagedResponse(dat);
                }
                else
                {
                    // if there is no intenet connectivity then we want to save on the server call
                    return getPagedResponse(null);
                }
            }
            catch (Exception)
            {
                // we are not sure why the exception is coming
                // mostly it should be a server/network issue as we are checking for the internet connectivity elsewhere
                // hence just returning with no details
                return getPagedResponse(null);
            }
        }

        #endregion
    }
}
