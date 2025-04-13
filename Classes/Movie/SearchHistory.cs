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

        public SearchHistory()
        {
            
        }

       public List<string> GetSearchHistory()
        {
            return searchHistory;
        }

       public void SetSearchHistory(List<string> list)
        {
            searchHistory = list;
        }

       public void AddSearchToHistory(string searchTerm)
        {
            if (!searchHistory.Contains(searchTerm))
            {
                searchHistory.Insert(0, searchTerm);
            }
        }
    }
}
