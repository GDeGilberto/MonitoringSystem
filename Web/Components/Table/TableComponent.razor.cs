using Microsoft.AspNetCore.Components;

namespace Web.Components.Table
{
    public partial class TableComponent<TViewModel> : ComponentBase
    {
        [Parameter] public bool IsLoading { get; set; } = false;
        [Parameter] public RenderFragment? FilterTemplate { get; set; }
        [Parameter] public IEnumerable<string> Titles { get; set; } = Enumerable.Empty<string>();
        [Parameter] public IEnumerable<TViewModel> Data { get; set; } = Enumerable.Empty<TViewModel>();
        [Parameter] public bool paginated { get; set; } = true;

        public string searchText = string.Empty;
        private string selectedOption = string.Empty;
        
        private int currentPage = 1;
        private int pageSize = 10;
        private bool isTableVisible = true;
        private int totalPages => (int)Math.Ceiling((double)DataFiltered.Count / pageSize);
        private int StartRecord => DataFiltered.Count == 0 ? 0 : ((currentPage - 1) * pageSize) + 1;
        private int EndRecord => Math.Min(currentPage * pageSize, DataFiltered.Count);

        private List<TViewModel> DataFiltered =>
            Data == null || !Data.Any()
                ? new List<TViewModel>()
                : Data
            .Where(r => string.IsNullOrEmpty(searchText) && string.IsNullOrEmpty(selectedOption))
            .ToList();

        private IEnumerable<TViewModel> PagedData =>
            DataFiltered.Skip((currentPage -1) * pageSize).Take(pageSize);

        private void GoToPage(int page)
        {
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;
            currentPage = page;
        }

        private async Task GoToPageWithFade(int page)
        {
            isTableVisible = false;
            StateHasChanged();
            await Task.Delay(150);
            GoToPage(page);
            isTableVisible = true;
            StateHasChanged();
        }
    }
}
