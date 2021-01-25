using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using DigitalArchiveAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArchiveAPI.AzureServices
{
    /// <summary>
    /// Encapsulates methods for interacting with Azure Search service.
    /// </summary>
    public class AzureSearch
    {
        #region ===== STATIC DECLARATIONS
        /// <summary>
        /// The maximum page size allowed to be requested from the search service.
        /// </summary>
        private const int MAX_PAGE_SIZE = 500;

        /// <summary>
        /// The name of the Azure Search index which will contain the biography (table of contents) data.
        /// </summary>
        private const string BIOGRAPHY_INDEX = "biographies";

        /// <summary>
        /// The name of the Azure Search index which will contain the story (segment) data.
        /// </summary>
        private const string STORY_INDEX = "stories";

        /// <summary>
        /// The filter piece to return only ScienceMakers, used for both biography and story calls.
        /// </summary>
        /// <remarks>this assumes isScienceMaker is defined and populated correctly for both biographies and stories</remarks>
        private const string FILTER_TO_SCIENCEMAKERS_ONLY = "isScienceMaker eq true";

#if SCIENCEMAKERS_ONLY
        /// <summary>
        /// The maker ID, i.e., category ID, to return only ScienceMakers, used for both biography and story calls.
        /// </summary>
        /// <remarks>This assumes ScienceMakers category numeric ID is the same for biography and story (and is 37).
        /// See biographyFacets.json and storyFacets.json to confirm.</remarks>
        private const string SCIENCEMAKERS_SUBSET_ID = "37"; // used with makerCategories, i.e., makerFacets // NOTE: perhaps tacking on: isScienceMaker eq true would work

#endif

        /// <summary>
        /// The streamlined set of fields to return when doing a biography search.
        /// </summary>
        private static readonly string[] StreamlinedBiographyFieldsSet =
            new[] { "biographyID", "accession", "birthDate", "preferredName" };

        /// <summary>
        /// The streamline set of fields to return when doing a story search.
        /// </summary>
        private static readonly string[] StreamlinedStoryFieldsSet =
            new[] { "storyID", "biographyID", "sessionOrder", "tapeOrder", "storyOrder", "duration", "interviewDate", "title" };

        /// <summary>
        /// The list of facets to be returned as part of any biography search result.
        /// </summary>
        private static readonly string[] BiographySearchFacets =
            new[] { "lastInitial,count:26", "gender", "birthYear,interval:10", "birthState, count:52", "makerCategories,count:15", "occupationTypes" };

        /// <summary>
        /// The list of facets to be returned as part of any story search result.
        /// </summary>
        private static readonly string[] StorySearchFacets =
            new string[] { "gender", "birthYear,interval:10", "makerCategories,count:15", "occupationTypes", "entityStates, count:52", "entityCountries", "entityOrganizations", "entityDecades", "entityYears" };
        #endregion == STATIC DECLARATIONS

        #region ===== INSTANCE DECLARATIONS
        /// <summary>
        /// A cached reference to the Azure Search service.
        /// </summary>
        private readonly SearchServiceClient serviceClient;

        /// <summary>
        /// A cached reference to the client used to search the biography index.
        /// </summary>
        private readonly ISearchIndexClient biographyIndex;

        /// <summary>
        /// A cached reference to the client used to search the story index.
        /// </summary>
        private readonly ISearchIndexClient storyIndex;

        #endregion == INSTANCE DECLARATIONS

        #region ===== CONSTRUCTOR
        /// <summary>
        /// Static constructor initializes the Azure Search context.
        /// </summary>
        /// <remarks>
        /// Following new pattern from:
        /// https://docs.microsoft.com/en-us/azure/search/search-get-started-dotnet
        /// </remarks>
        public AzureSearch(string serviceName, string apiKey)
        {
            serviceClient = new SearchServiceClient(serviceName, new SearchCredentials(apiKey));
            biographyIndex = serviceClient.Indexes.GetClient(BIOGRAPHY_INDEX);
            storyIndex = serviceClient.Indexes.GetClient(STORY_INDEX);
        }
        #endregion == CONSTRUCTOR

        #region ===== BIOGRAPHY RELATED METHODS
        /// <summary>
        /// Perform full-text search of biograpy (collection) index and apply facets as given.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="pageSize">Number of items to return in the current result set.</param>
        /// <param name="currentPage">Which page of results to return based on page size.</param>
        /// <param name="searchFields">A comma-separated list of fields to be searched.</param>
        /// <param name="genderFacet">Filters results based on given gender.</param>
        /// <param name="birthStateFacet">Filters results based on given birth state (U.S.)</param>
        /// <param name="birthYearFacet">Filters results based on given birth year.</param>
        /// <param name="makerFacet">Filters results based on the given comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filters results based on the given comma-separated list of job types.</param>
        /// <param name="lastInitialFacet">Filter results by first letter of last name, e.g., C is filtering to all last names starting with C.</param>
        /// <param name="sortField">Sort results by this field (with no sort qualifier added if null).</param>
        /// <param name="sortInDescendingOrder">If sortField is given, sort results in descending order if true, ascending order if false.</param>
        /// <returns>A DocumentSearchResult object containing the results of the specified search.</returns>
        internal async Task<DocumentSearchResult<BiographySearchDocument>> BiographySearch(
            string query, int pageSize, int currentPage, string searchFields, string genderFacet, 
            string birthStateFacet, string birthYearFacet, string makerFacet, string jobFacet, 
            string lastInitialFacet, string sortField, bool sortInDescendingOrder)
        {
            List<string> fieldsToSearch;
            if (searchFields == "all")
                fieldsToSearch = new List<string>() { "descriptionShort", "lastName", "preferredName", "accession" };
            else
                fieldsToSearch = new List<string>(searchFields.Split(','));

            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.All,
                SearchFields = fieldsToSearch,
                Top = pageSize,
                Skip = (currentPage - 1) * pageSize,
                IncludeTotalResultCount = true,
                HighlightFields = fieldsToSearch,
                HighlightPreTag = "<em>",
                HighlightPostTag = "</em>",
                Facets = BiographySearchFacets,
                Select = StreamlinedBiographyFieldsSet
            };

            sp.Filter = GetBiographySearchFilterFromFacets(lastInitialFacet, genderFacet, birthStateFacet, birthYearFacet, makerFacet, jobFacet);

            if (!string.IsNullOrWhiteSpace(sortField))
            {
                string sortQualifier = sortInDescendingOrder ? sortField + " desc" : sortField + " asc";
                sp.OrderBy = new List<String> { sortQualifier };
            }

            return await biographyIndex.Documents.SearchAsync<BiographySearchDocument>(query, sp).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of biographies corresponding to people born on the current day.
        /// </summary>
        /// <param name="pageSize">Number of items to return in the current result set.</param>
        /// <param name="currentPage">Which page of results to return based on page size.</param>
        /// <param name="genderFacet">Filters results based on given gender.</param>
        /// <param name="birthYearFacet">Filters results based on given birth year.</param>
        /// <param name="makerFacet">Filters results based on the given comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filters results based on the given comma-separated list of job types.</param>
        /// <param name="lastInitialFacet">Filter results by first letter of last name, e.g., C is filtering to all last names starting with C.</param>
        /// <param name="dateToday">ISO 8601 date string specifying a specific date; defaults to today if not specified.</param>
        /// <returns>A DocumentSearchResult object containing the biographies of people born on the current day, sorted oldest to youngest.</returns>
        internal async Task<DocumentSearchResult<BiographySearchDocument>> GetPeopleBornThisDay(int pageSize, int currentPage, string genderFacet,
            string birthYearFacet, string makerFacet, string jobFacet, string lastInitialFacet, string dateToday)
        {
            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.Any,
                Top = pageSize,
                Skip = (currentPage - 1) * pageSize,
                IncludeTotalResultCount = true,
                Facets = BiographySearchFacets,
                Select = StreamlinedBiographyFieldsSet
            };

            DateTime today;
            if (String.IsNullOrEmpty(dateToday))
                today = DateTime.Today;
            else
                today = DateTime.Parse(dateToday, null, System.Globalization.DateTimeStyles.RoundtripKind);

            sp.Filter = GetBiographySearchFilterFromFacets(lastInitialFacet, genderFacet, null, birthYearFacet, makerFacet, jobFacet);

            if (sp.Filter != null) sp.Filter += " and ";

            // Add in the "born this day" filter:
            sp.Filter += $"(birthMonth eq {today.Month} and birthDay eq {today.Day})";

            // Order people oldest to youngest:
            sp.OrderBy = new List<String> { "birthYear asc" };

            return await biographyIndex.Documents.SearchAsync<BiographySearchDocument>(string.Empty, sp).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of biographies corresponding to people born in the current week.
        /// </summary>
        /// <param name="pageSize">Number of items to return in the current result set.</param>
        /// <param name="currentPage">Which page of results to return based on page size.</param>
        /// <param name="genderFacet">Filters results based on given gender.</param>
        /// <param name="birthYearFacet">Filters results based on given birth year.</param>
        /// <param name="makerFacet">Filters results based on the given comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filters results based on the given comma-separated list of job types.</param>
        /// <param name="lastInitialFacet">Filter results by first letter of last name, e.g., C is filtering to all last names starting with C.</param>
        /// <param name="dateToday">ISO 8601 date string specifying a specific date; defaults to today if not specified.</param>
        /// <returns>
        /// A DocumentSearchResult object containing the biographies of people born in the current week, sorted by 
        /// day (mm-dd) of birth (and then oldest to youngest for same mm-dd).
        /// </returns>
        internal async Task<DocumentSearchResult<BiographySearchDocument>> GetPeopleBornThisWeek(int pageSize, int currentPage, string genderFacet,
            string birthYearFacet, string makerFacet, string jobFacet, string lastInitialFacet, string dateToday)
        {
            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.Any,
                Top = pageSize,
                Skip = (currentPage - 1) * pageSize,
                IncludeTotalResultCount = true,
                Facets = BiographySearchFacets,
                Select = StreamlinedBiographyFieldsSet
            };

            // Compute the week as Sunday through Saturday bounding the current date.
            DateTime today;
            if (String.IsNullOrEmpty(dateToday))
                today = DateTime.Today;
            else
                today = DateTime.Parse(dateToday, null, System.Globalization.DateTimeStyles.RoundtripKind);

            DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            sp.Filter = GetBiographySearchFilterFromFacets(lastInitialFacet, genderFacet, null, birthYearFacet, makerFacet, jobFacet);

            if (sp.Filter != null) sp.Filter += " and ";

            if (startOfWeek.Month == endOfWeek.Month)
            {
                sp.Filter += $"(birthMonth eq {startOfWeek.Month} and birthDay ge {startOfWeek.Day} and birthDay le {endOfWeek.Day})";

                // Order people by the day of their birth, 1 to 31 and then oldest to youngest:
                sp.OrderBy = new List<String> { "birthDay asc", "birthYear asc" };
            }
            else
            {
                // Week spans across months so must adjust query accordingly
                sp.Filter += $"((birthMonth eq {startOfWeek.Month} and birthDay ge {startOfWeek.Day}) or (birthMonth eq {endOfWeek.Month} and birthDay le {endOfWeek.Day}))";

                // Order people by the month first, and then day of their birth, 1 to 31, and then oldest to youngest:
                sp.OrderBy = new List<String> { "birthMonth asc", "birthDay asc", "birthYear asc" };
            }

            return await biographyIndex.Documents.SearchAsync<BiographySearchDocument>(string.Empty, sp).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of biographies corresponding to people born in the current month.
        /// </summary>
        /// <param name="pageSize">Number of items to return in the current result set.</param>
        /// <param name="currentPage">Which page of results to return based on page size.</param>
        /// <param name="genderFacet">Filters results based on given gender.</param>
        /// <param name="birthYearFacet">Filters results based on given birth year.</param>
        /// <param name="makerFacet">Filters results based on the given comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filters results based on the given comma-separated list of job types.</param>
        /// <param name="lastInitialFacet">Filter results by first letter of last name, e.g., C is filtering to all last names starting with C.</param>
        /// <param name="dateToday">String equivalent of today's date.</param>
        /// <returns>A DocumentSearchResult object containing the biographies of people born in the current month, 
        /// ordered by birth day and then oldest to youngest on same birth day.</returns>
        internal async Task<DocumentSearchResult<BiographySearchDocument>> GetPeopleBornThisMonth(int pageSize, int currentPage, string genderFacet,
            string birthYearFacet, string makerFacet, string jobFacet, string lastInitialFacet, string dateToday)
        {
            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.Any,
                Top = pageSize,
                Skip = (currentPage - 1) * pageSize,
                IncludeTotalResultCount = true,
                Facets = BiographySearchFacets,
                Select = StreamlinedBiographyFieldsSet
            };

            // Compute the month as the full month bounding the current date.
            DateTime today;
            if (String.IsNullOrEmpty(dateToday))
                today = DateTime.Today;
            else
                today = DateTime.Parse(dateToday, null, System.Globalization.DateTimeStyles.RoundtripKind);

            sp.Filter = GetBiographySearchFilterFromFacets(lastInitialFacet, genderFacet, null, birthYearFacet, makerFacet, jobFacet);

            if (sp.Filter != null) sp.Filter += " and ";

            sp.Filter += $"birthMonth eq {today.Month}";
                
            //    string.Format("(birthMonth eq {0} and birthDay ge {1} and birthDay le {2})",
            //        today.Month, 1, 31); // don't worry about nonsensical dates like April 31 - they shouldn't be in data store

            // Order people by the day of their birth, 1 to 31 and then oldest to youngest:
            sp.OrderBy = new List<String> { "birthDay asc", "birthYear asc" };

            return await biographyIndex.Documents.SearchAsync<BiographySearchDocument>(string.Empty, sp).ConfigureAwait(false);
        }


        #endregion == BIOGRAPHY RELATED METHODS

        #region == STORY RELATED METHODS
        /// <summary>
        /// Perform full-text search of the story index and apply facets as given.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="pageSize">Number of results to return per call.</param>
        /// <param name="currentPage">The 1-based page of results to return.</param>
        /// <param name="parentBiographyID">Filter stories by a biography ID.</param>
        /// <param name="searchFields">A comma-separated list of fields to be searched.</param>
        /// <param name="interviewYearFilterLowerBound">Lower bound on returned stories' interview year, ignored if 0.</param>
        /// <param name="interviewYearFilterUpperBound">Upper bound on returned stories' interview year, ignored if 0.</param>
        /// <param name="genderFacet">Filter stories by gender.</param>
        /// <param name="birthYearFacet">Filter stories by birth year.</param>
        /// <param name="makerFacet">Filter stories by comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filter stories by comma-separated list of job types.</param>
        /// <param name="entityStateFacet">A comma-separated list of U.S. states; null if not used.</param>
        /// <param name="entityCountryFacet">A comma-separated list of countries; null if not used.</param>
        /// <param name="entityOrgFacet">A comma-separated list of organizations; null if not used.</param>
        /// <param name="entityDecadeFacet">A comma-separated list of decades; null if not used.</param>
        /// <param name="entityYearFacet">A comma-separated list of years; null if not used.</param>
        /// <param name="sortField">Sort results by this field (with no sort qualifier added if null).</param>
        /// <param name="sortInDescendingOrder">If sortField is given, sort results in descending order if true, ascending order if false.</param>
        /// <returns>A DocumentSearchResult object containing the results of the specified search.</returns>
        /// <remarks>If searchFields is "all" it will be taken to be title,transcript.</remarks>
        internal async Task<DocumentSearchResult<StorySearchDocument>> StorySearch(
            string query, int pageSize, int currentPage, string parentBiographyID, string searchFields, 
            int interviewYearFilterLowerBound, int interviewYearFilterUpperBound, string genderFacet, 
            string birthYearFacet, string makerFacet, string jobFacet, string entityStateFacet, 
            string entityCountryFacet, string entityOrgFacet, string entityDecadeFacet, 
            string entityYearFacet, string sortField, bool sortInDescendingOrder)
        { 
            List<string> fieldsToSearch;

            if (searchFields == "all")
                fieldsToSearch = new List<string>(new string[] { "title", "transcript" });
            else
                fieldsToSearch = new List<string>(searchFields.Split(','));

            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.All,
                SearchFields = fieldsToSearch,
                Top = pageSize,
                Skip = (currentPage - 1) * pageSize,
                IncludeTotalResultCount = true,
                HighlightFields = fieldsToSearch,
                HighlightPreTag = "<em>",
                HighlightPostTag = "</em>",
                Facets = StorySearchFacets,
                Select = StreamlinedStoryFieldsSet
            };

            sp.Filter = GetStorySearchFilterFromFacets(genderFacet, birthYearFacet, makerFacet, jobFacet, entityStateFacet,
                entityCountryFacet, entityOrgFacet, entityDecadeFacet, entityYearFacet, parentBiographyID, interviewYearFilterUpperBound, interviewYearFilterLowerBound);

            if (!string.IsNullOrWhiteSpace(sortField))
            {
                string sortQualifier = sortInDescendingOrder ? sortField + " desc" : sortField + " asc";
                sp.OrderBy = new List<String> { sortQualifier };
            }

            return await storyIndex.Documents.SearchAsync<StorySearchDocument>(query, sp).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform a tag-based search of the story index and apply facets as given.
        /// </summary>
        /// <param name="tags">A comma-separated list of tags.</param>
        /// <param name="pageSize">Number of results to return per call.</param>
        /// <param name="currentPage">The 1-based page of results to return.</param>
        /// <param name="genderFacet">Filter stories by gender.</param>
        /// <param name="yearFacet">Filter stories by birth year.</param>
        /// <param name="makerFacet">Filter stories by comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filter stories by comma-separated list of job types.</param>
        /// <param name="entityStateFacet">A comma-separated list of U.S. states; null if not used.</param>
        /// <param name="entityCountryFacet">A comma-separated list of countries; null if not used.</param>
        /// <param name="entityOrgFacet">A comma-separated list of organizations; null if not used.</param>
        /// <param name="entityDecadeFacet">A comma-separated list of decades; null if not used.</param>
        /// <param name="entityYearFacet">A comma-separated list of years; null if not used.</param>
        /// <param name="sortField">Sort results by this field (with no sort qualifier added if null).</param>
        /// <param name="sortInDescendingOrder">If sortField is given, sort results in descending order if true, ascending order if false.</param>
        /// <returns>A DocumentSearchResult object containing the results of the specified tag search.</returns>
        internal async Task<DocumentSearchResult<StorySearchDocument>> StorySearchByTags(
            string tags, int pageSize, int currentPage, string genderFacet, string yearFacet, string makerFacet, string jobFacet,
            string entityStateFacet, string entityCountryFacet, string entityOrgFacet, string entityDecadeFacet,
            string entityYearFacet, string sortField, bool sortInDescendingOrder)
        {
            SearchParameters sp = new SearchParameters()
            {
                Top = pageSize,
                Skip = (currentPage - 1) * pageSize,
                IncludeTotalResultCount = true,
                Facets = StorySearchFacets,
                Select = StreamlinedStoryFieldsSet
            };

            sp.Filter = GetTagSearchFilterFromFacets(genderFacet, yearFacet, makerFacet, jobFacet, entityStateFacet,
                entityCountryFacet, entityOrgFacet, entityDecadeFacet, entityYearFacet, tags);
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                string sortQualifier = sortInDescendingOrder ? sortField + " desc" : sortField + " asc";
                sp.OrderBy = new List<String> { sortQualifier };
            }

            return await storyIndex.Documents.SearchAsync<StorySearchDocument>("*", sp).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the story documents corresponding to the given list.
        /// </summary>
        /// <param name="storyIDs">A list of StoryIDs to be retrieved.</param>
        /// <param name="genderFacet">Filters results based on given gender.</param>
        /// <param name="yearFacet">Filters results based on given birth year.</param>
        /// <param name="makerFacet">Filters results based on the given comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filters results based on the given comma-separated list of job types.</param>
        /// <param name="entityStateFacet">A comma-separated list of U.S. states; null if not used.</param>
        /// <param name="entityCountryFacet">A comma-separated list of countries; null if not used.</param>
        /// <param name="entityOrgFacet">A comma-separated list of organizations; null if not used.</param>
        /// <param name="entityDecadeFacet">A comma-separated list of decades; null if not used.</param>
        /// <param name="entityYearFacet">A comma-separated list of years; null if not used.</param>
        /// <returns>A DocumentSearchResult object containing an ordered list of stories.</returns>
        internal async Task<DocumentSearchResult<StorySearchDocument>> StorySet(
            List<string> storyIDs, string genderFacet, string yearFacet, string makerFacet, string jobFacet, 
            string entityStateFacet, string entityCountryFacet, string entityOrgFacet, string entityDecadeFacet,
            string entityYearFacet)
        {
            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.Any,
                SearchFields = new List<string> { "storyID" },
                Top = MAX_PAGE_SIZE,
                IncludeTotalResultCount = true,
                Facets = StorySearchFacets,
                Select = StreamlinedStoryFieldsSet
            };

           sp.Filter = GetStorySearchFilterFromFacets(genderFacet, yearFacet, makerFacet, jobFacet, 
                entityStateFacet, entityCountryFacet, entityOrgFacet, entityDecadeFacet, entityYearFacet);

            var query = string.Join(" ", storyIDs);
            var searchResult = await storyIndex.Documents.SearchAsync<StorySearchDocument>(query, sp).ConfigureAwait(false);

            // Reordering the results per the given list ordering using LINQ:
            // http://stackoverflow.com/questions/3945935/sort-one-list-by-another
            var orderedByIDList = from storyID in storyIDs
                                  join result in searchResult.Results
                                  on storyID equals result.Document.StoryID
                                  select result;

            // DocumentSearchResult properties are immutable, so create a new one to
            // hold the re-ordered results
            return new DocumentSearchResult<StorySearchDocument>(
                orderedByIDList.ToList(), 
                searchResult.Count, 
                searchResult.Coverage, 
                searchResult.Facets, 
                searchResult.ContinuationToken);
        }

        /// <summary>
        /// Return the count of stories matching each tag filtered by the given list of tags.
        /// </summary>
        /// <param name="tags">A comma separate list of tags.</param>
        /// <returns>A DocumentSearchResult object containing the results of the search operation.</returns>
        internal async Task<DocumentSearchResult<StorySearchDocument>> StorySearchTagCounts(string tags)
        {
            // Per documentation: set the requested number of values higher than the number of existing
            // tag values to force Azure Search to do a deep search  resulting in more accurate counts.
            //
            // See: Make sure you get accurate facet counts
            // https://docs.microsoft.com/en-us/azure/search/search-faceted-navigation
            var tagFacet = new List<string> { "tags, count:200" };

            SearchParameters sp = new SearchParameters()
            {
                IncludeTotalResultCount = true,
                Top = 0,                            // We don't need documents, just the tag (facet) counts.
                Facets = tagFacet,                  // We only need the Tags facet for counts
                Select = new List<string> { }       // We don't need documents returned.                    
            };
#if SCIENCEMAKERS_ONLY
            string filterStart = GetFilterForStringCollectionFacet("tags", tags);
            if (filterStart != null)
                sp.Filter = filterStart + " and " + FILTER_TO_SCIENCEMAKERS_ONLY;
            else
                sp.Filter = FILTER_TO_SCIENCEMAKERS_ONLY;
#else
            sp.Filter = GetFilterForStringCollectionFacet("tags", tags);
#endif
            return await storyIndex.Documents.SearchAsync<StorySearchDocument>("*", sp).ConfigureAwait(false);
        }
#endregion == STORY RELATED METHODS

        #region ===== ADDITIONAL SEARCH METHODS
        /// <summary>
        /// Perform language analysis (tokenization, lemmatization, etc...) on the given text.
        /// </summary>
        /// <param name="text">The text to be analyzed.</param>
        /// <returns>The AnalyzeResult instance returned by the API on success; null on failure.</returns>
        internal async Task<AnalyzeResult> AnalyzeText(string text)
        {
            return await serviceClient.Indexes.AnalyzeAsync(STORY_INDEX, new AnalyzeRequest(text, AnalyzerName.EnMicrosoft)).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns summary information about the corpus
        /// </summary>
        /// <returns></returns>
        internal async Task<CorpusInfo> CorpusMetrics()
        {
            return new CorpusInfo
            {
                LastUpdated = DateTime.Now,
                Biographies = await BiographyCounts().ConfigureAwait(false),
                Stories = await StoryCounts().ConfigureAwait(false)
            };
        }

        /// <summary>
        /// Get the count of all biographies and the subset with tags.
        /// </summary>
        /// <returns>A SetCounts instance with resulting counts.</returns>
        private async Task<SetCounts> BiographyCounts()
        {
            var results = new SetCounts();

            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.All,
                SearchFields = new List<string>() { "accession" },
                Top = 0,
                IncludeTotalResultCount = true,
            };

            results.All = (await biographyIndex.Documents.SearchAsync<BiographySearchDocument>("*", sp).ConfigureAwait(false)).Count;

            sp.Filter = "isTagged eq true";
            results.Tagged = (await biographyIndex.Documents.SearchAsync<BiographySearchDocument>("*", sp).ConfigureAwait(false)).Count;
            // NOTE: Always populating this count, not just for some versions:
            sp.Filter = FILTER_TO_SCIENCEMAKERS_ONLY; 
            results.ScienceMakerCount = (await biographyIndex.Documents.SearchAsync<BiographySearchDocument>("*", sp).ConfigureAwait(false)).Count;
            return results;
        }

        /// <summary>
        /// Get the count of all stories and the subset with tags.
        /// </summary>
        /// <returns>A SetCounts instance with resulting counts.</returns>
        private async Task<SetCounts> StoryCounts()
        {
            var results = new SetCounts();

            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.All,
                SearchFields = new List<string>() { "storyID" },
                Top = 0,
                IncludeTotalResultCount = true,
            };

            results.All = (await storyIndex.Documents.SearchAsync<StorySearchDocument>("*", sp).ConfigureAwait(false)).Count;

            sp.Filter = "isTagged eq true";
            results.Tagged = (await storyIndex.Documents.SearchAsync<StorySearchDocument>("*", sp).ConfigureAwait(false)).Count;
            // NOTE: Always populating this count, not just for some versions:
            sp.Filter = FILTER_TO_SCIENCEMAKERS_ONLY;
            results.ScienceMakerCount = (await storyIndex.Documents.SearchAsync<StorySearchDocument>("*", sp).ConfigureAwait(false)).Count;
            return results;
        }
        #endregion == ADDITIONAL SEARCH METHODS

        #region ===== PRIVATE STATIC FILTER METHODS
        /// <summary>
        /// Formulates a biography search filter expression for the given set of facets.
        /// </summary>
        /// <param name="lastInitialFacet">Singular first letter of subject's last name; null if not used.</param>
        /// <param name="genderFacet">Singular gender specifier; null if not used.</param>
        /// <param name="birthStateFacet">Two character specifying U.S. state of birth; null if not used.</param>
        /// <param name="birthYearFacet">Four digit number specifying year of birth; null if not used.</param>
        /// <param name="makerFacet">A comma-separated list of maker categories; null if not used.</param>
        /// <param name="jobFacet">A comma-separated list of job types; null if not used.</param>
        /// <returns>An ODATA compliant search filter expression as a string.</returns>
        private static string GetBiographySearchFilterFromFacets(
            string lastInitialFacet, string genderFacet, string birthStateFacet, string birthYearFacet, string makerFacet, string jobFacet)
        {
            string filter = null;

            if (!string.IsNullOrEmpty(genderFacet))
            {
                // Always false, so don't bother... if (filter != null) filter += " and ";
                filter += GetFilterForStringFacet("gender", genderFacet);
            }

            if (!string.IsNullOrEmpty(birthStateFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringFacet("birthState", birthStateFacet);
            }

            if (!string.IsNullOrEmpty(birthYearFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterFromBirthYearFacet(birthYearFacet);
            }
#if SCIENCEMAKERS_ONLY
            string makerFacetPlus;
            if (makerFacet != null && makerFacet.Trim().Length > 0)
                makerFacetPlus = makerFacet += "," + SCIENCEMAKERS_SUBSET_ID;
            else
                makerFacetPlus = SCIENCEMAKERS_SUBSET_ID;
            if (filter != null) filter += " and ";
            filter += GetFilterForStringCollectionFacet("makerCategories", makerFacetPlus);
#else
            if (!string.IsNullOrEmpty(makerFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringCollectionFacet("makerCategories", makerFacet);
            }
#endif
            if (!string.IsNullOrEmpty(jobFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringCollectionFacet("occupationTypes", jobFacet);
            }

            if (!string.IsNullOrEmpty(lastInitialFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringFacet("lastInitial", lastInitialFacet);
            }

            return filter;
        }

        /// <summary>
        /// Formulates a story search filter expression for the given set of facets.
        /// </summary>
        /// <param name="genderFacet">Singular gender specifier; null if not used.</param>
        /// <param name="birthYearFacet">Four digit number specifying year of birth; null if not used.</param>
        /// <param name="makerFacet">A comma-separated list of maker categories; null if not used.</param>
        /// <param name="jobFacet">A comma-separated list of job types; null if not used.</param>
        /// <param name="entityStateFacet">A comma-separated list of U.S. states; null if not used.</param>
        /// <param name="entityCountryFacet">A comma-separated list of countries; null if not used.</param>
        /// <param name="entityOrgFacet">A comma-separated list of organizations; null if not used.</param>
        /// <param name="entityDecadeFacet">A comma-separated list of decades; null if not used.</param>
        /// <param name="entityYearFacet">A comma-separated list of years; null if not used.</param>
        /// <returns>An ODATA compliant search filter expression as a string.</returns>
        private static string GetStorySearchFilterFromFacets(
            string genderFacet, string birthYearFacet, string makerFacet, string jobFacet, string entityStateFacet,
            string entityCountryFacet, string entityOrgFacet, string entityDecadeFacet, string entityYearFacet)
        {
            string filter = null;

            if (!string.IsNullOrEmpty(genderFacet))
            {
                // Always false, so don't bother... if (filter != null) filter += " and ";
                filter += GetFilterForStringFacet("gender", genderFacet);
            }

            if (!string.IsNullOrEmpty(birthYearFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterFromBirthYearFacet(birthYearFacet);
            }

#if SCIENCEMAKERS_ONLY
            // Get filter from maker categories, but filter further by maker within ScienceMakers ALWAYS
            string makerFacetPlus;
            if (makerFacet != null && makerFacet.Trim().Length > 0)
                makerFacetPlus = makerFacet += "," + SCIENCEMAKERS_SUBSET_ID;
            else
                makerFacetPlus = SCIENCEMAKERS_SUBSET_ID;
            if (filter != null) filter += " and ";
            filter += GetFilterForStringCollectionFacet("makerCategories", makerFacetPlus);
#else
            if (!string.IsNullOrEmpty(makerFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringCollectionFacet("makerCategories", makerFacet);
            }
#endif
            if (!string.IsNullOrEmpty(jobFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringCollectionFacet("occupationTypes", jobFacet);
            }

            if (!string.IsNullOrEmpty(entityStateFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringCollectionFacet("entityStates", entityStateFacet);
            }

            if (!string.IsNullOrEmpty(entityCountryFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringCollectionFacet("entityCountries", entityCountryFacet);
            }

            if (!string.IsNullOrEmpty(entityOrgFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringCollectionFacet("entityOrganizations", entityOrgFacet);
            }

            if (!string.IsNullOrEmpty(entityDecadeFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForIntegerCollectionFacet("entityDecades", entityDecadeFacet);
            }

            if (!string.IsNullOrEmpty(entityYearFacet))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForIntegerCollectionFacet("entityYears", entityYearFacet);
            }

            return filter;
        }

        /// <summary>
        /// Formulates a story search filter expression for the given set of facets.
        /// </summary>
        /// <param name="genderFacet">Singular gender specifier; null if not used.</param>
        /// <param name="birthYearFacet">Four digit number specifying year of birth; null if not used.</param>
        /// <param name="makerFacet">A comma-separated list of maker categories; null if not used.</param>
        /// <param name="jobFacet">A comma-separated list of job types; null if not used.</param>
        /// <param name="entityStateFacet">A comma-separated list of U.S. states; null if not used.</param>
        /// <param name="entityCountryFacet">A comma-separated list of countries; null if not used.</param>
        /// <param name="entityOrgFacet">A comma-separated list of organizations; null if not used.</param>
        /// <param name="entityDecadeFacet">A comma-separated list of decades; null if not used.</param>
        /// <param name="entityYearFacet">A comma-separated list of years; null if not used.</param>
        /// <param name="parentBiographyID">Singular biography identifier; null if not used.</param>
        /// <param name="interviewYearUpperBound">Upper bound of interview year range; 0 if not used.</param>
        /// <param name="interviewYearLowerBound">Lower bound of interview year range; 0 if not used.</param>
        /// <returns>An ODATA compliant search filter expression as a string.</returns>
        private static string GetStorySearchFilterFromFacets(
            string genderFacet, string birthYearFacet, string makerFacet, string jobFacet, string entityStateFacet,
            string entityCountryFacet, string entityOrgFacet, string entityDecadeFacet, string entityYearFacet,
            string parentBiographyID, int interviewYearUpperBound, int interviewYearLowerBound)
        {
            string filter = GetStorySearchFilterFromFacets(genderFacet, birthYearFacet, makerFacet, jobFacet,
                entityStateFacet, entityCountryFacet, entityOrgFacet, entityDecadeFacet, entityYearFacet);

            if (!string.IsNullOrEmpty(parentBiographyID))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringFacet("biographyID", parentBiographyID);
            }

            if (interviewYearLowerBound != 0 || interviewYearUpperBound != 0)
            {
                if(filter != null) filter += " and ";
                filter += GetFilterFromInterviewYearRange(interviewYearLowerBound, interviewYearUpperBound);
            }

            return filter;
        }

        /// <summary>
        /// Formulates a tag search filter expression for the given set of facets.
        /// </summary>
        /// <param name="genderFacet">Singular gender specifier; null if not used.</param>
        /// <param name="birthYearFacet">Four digit number specifying year of birth; null if not used.</param>
        /// <param name="makerFacet">A comma-separated list of maker categories; null if not used.</param>
        /// <param name="jobFacet">A comma-separated list of job types; null if not used.</param>
        /// <param name="entityStateFacet">A comma-separated list of U.S. states; null if not used.</param>
        /// <param name="entityCountryFacet">A comma-separated list of countries; null if not used.</param>
        /// <param name="entityOrgFacet">A comma-separated list of organizations; null if not used.</param>
        /// <param name="entityDecadeFacet">A comma-separated list of decades; null if not used.</param>
        /// <param name="entityYearFacet">A comma-separated list of years; null if not used.</param>
        /// <param name="tags">A comma-separate list of tags; null if not used.</param>
        /// <returns>An ODATA compliant search filter expression as a string.</returns>
        private static string GetTagSearchFilterFromFacets(
            string genderFacet, string birthYearFacet, string makerFacet, string jobFacet, string entityStateFacet, 
            string entityCountryFacet, string entityOrgFacet, string entityDecadeFacet, string entityYearFacet, 
            string tags)
        {
            string filter = GetStorySearchFilterFromFacets(genderFacet, birthYearFacet, makerFacet, jobFacet,
                entityStateFacet, entityCountryFacet, entityOrgFacet, entityDecadeFacet, entityYearFacet);

            if (!string.IsNullOrEmpty(tags))
            {
                if (filter != null) filter += " and ";
                filter += GetFilterForStringCollectionFacet("tags", tags);
            }

            return filter;
        }
        #endregion == PRIVATE STATIC FILTER METHODS

        #region ===== PRIVATE STATIC HELPER METHODS
        /// <summary>
        /// Formulates an ODATA search filter expression for the given facet and list of integer values.
        /// </summary>
        /// <param name="facetName">The name of the facet as defined in the search index.</param>
        /// <param name="valueList">A comma-separated list of integer values.</param>
        /// <returns></returns>
        private static string GetFilterForIntegerCollectionFacet(string facetName, string valueList)
        {
            if (string.IsNullOrEmpty(valueList))
            {
                return null;
            }
            else
            {
                var expressions = new List<string>();

                var values = Array.ConvertAll(valueList.Split(','), v => v.Trim());
                foreach (var value in values)
                {
                    if (!String.IsNullOrEmpty(value))
                        expressions.Add($"{facetName}/any(v: v eq {value})");
                }

                return string.Join(" and ", expressions);
            }
        }

        /// <summary>
        /// Formulate an ODATA search filter expression for the given facet and list of string values.
        /// </summary>
        /// <param name="facetName">The name of the facet as defined in the search index.</param>
        /// <param name="valueList">A comma-separated list of string values.</param>
        /// <returns>An ODATA compliant search filter expression as a string.</returns>
        private static string GetFilterForStringCollectionFacet(string facetName, string valueList)
        {
            if (string.IsNullOrEmpty(valueList))
            {
                return null;
            }
            else
            {
                var expressions = new List<string>();

                var values = Array.ConvertAll(valueList.Split(','), v => v.Trim());
                foreach (var value in values)
                {
                    if (!String.IsNullOrEmpty(value))
                        expressions.Add($"{facetName}/any(v: v eq '{value}')");
                }

                return string.Join(" and ", expressions);
            }
        }

        /// <summary>
        /// Formulate an ODATA search filter expression for the given facet and value.
        /// </summary>
        /// <param name="facetName">The name of the facet as defined in the search index.</param>
        /// <param name="value">A singular value.</param>
        /// <returns>An ODATA compliant search filter expression as a string.</returns>
        private static string GetFilterForStringFacet(string facetName, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            else
            {
                return $"{facetName} eq '{value}'";
            }
        }

        /// <summary>
        /// Formulates an ODATA search filter expression for the given year.
        /// </summary>
        /// <param name="birthYearFacet">A singular year as a four-digit string.</param>
        /// <returns>An ODATA compliant search filter expression as a string.</returns>
        private static string GetFilterFromBirthYearFacet(string birthYearFacet)
        {
            if (string.IsNullOrEmpty(birthYearFacet))
            {
                return null;
            }
            else
            {
                // NOTE: this ASSUMES that we have interval:10 for birthYear in BiographySearchFacets, StorySearchFacets, or whatever
                // facet set is used to cluster birth years into decades.
                // Hence, if a decade is picked, any year in that range of [X, X+10) is valid.  Don't match X+10: use less than!
                if (int.TryParse(birthYearFacet, out int value))
                {
                    return $"birthYear ge {birthYearFacet} and birthYear lt {value + 10}";
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Formulates an ODATA search filter expression for the given range of interview years.
        /// </summary>
        /// <param name="yearStart">Starting year of range (inclusive).</param>
        /// <param name="yearEnd">Ending year of range (inclusive).</param>
        /// <returns>An ODATA compliant search filter expression as a string.</returns>
        private static string GetFilterFromInterviewYearRange(int yearStart, int yearEnd)
        {
            // Case 1: upper bound given, no lower bound:
            // interviewDate le yearEnd-12-31
            // Case 2: lower bound given, no upper bound:
            // interviewDate ge yearStart-01-01
            // Case 3: both bounds make sense:
            // interviewDate ge yearStart-01-01 and interviewDate le yearEnd-12-31
            // where for all cases UUUU is year of upper bound, LLLL is year of lower bound.
            string expression = null;

            if (yearStart != 0)
            {
                expression += $"interviewDate ge {yearStart}-01-01";
            }
            if (yearEnd != 0)
            {
                if (expression != null) expression += " and ";
                expression += $"interviewDate le {yearEnd}-12-31";
            }

            return expression;
        }
#endregion == PRIVATE HELPER METHODS
    }
}
