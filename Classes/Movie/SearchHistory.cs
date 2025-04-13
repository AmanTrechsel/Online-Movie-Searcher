using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online_Movie_Searcher.Classes.Movie
{
    internal class SearchHistory
    {
        private List<string> searchHistory = new();
        private readonly object historyLock = new();

        public SearchHistory()
        {
            
        }

       public List<string> GetSearchHistory()
        {
            lock(historyLock)
            {
                return searchHistory;
            }
        }

       public void SetSearchHistory(List<string> list)
        {
            searchHistory = list;
        }

       public void AddSearchToHistory(string searchTerm)
        {
            lock(historyLock)
            {
                if (!searchHistory.Contains(searchTerm))
                {
                    searchHistory.Insert(0, searchTerm);
                }
            }
        }
    }
}
