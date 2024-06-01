namespace RoboZZle.Offline;

using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using PCLStorage;

public sealed class LocalSolutions: ILocalSolutions {
    /// <summary>
    /// Creates the instance of local solutions from default file
    /// </summary>
    public static async Task<ILocalSolutions> Open(IFolder store) {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        IFile solutionsFile = await store
                                    .CreateFileAsync("solutions.txt",
                                                     CreationCollisionOption.OpenIfExists)
                                    .ConfigureAwait(false);
        var solutions = await Load(solutionsFile).ConfigureAwait(false);
        var result = new LocalSolutions(solutionsFile, solutions);

        DebugEx.WriteLine("local solutions arrived");

        return result;
    }

    /// <summary>
    /// Adds solution
    /// </summary>
    public async void Add(LocalSolution solution) {
        string? encodedProgram = solution.Program.Encode(false);
        string solutionDescriptor = string.Format(CultureInfo.InvariantCulture, "{0}\t{1}{2}",
                                                  solution.PuzzleID, encodedProgram,
                                                  Environment.NewLine);
        this.solutions.Add(solution);
        await this.storage.AppendAllTextAsync(solutionDescriptor).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets registered solutions
    /// </summary>
    public IEnumerable<LocalSolution> Solutions => this.solutions;

    readonly IFile storage;
    readonly ICollection<LocalSolution> solutions;

    #region Private implementation

    static LocalSolution ParseSolutionDescriptor(string solution) {
        int tab = solution.IndexOf('\t');
        if (tab < 0)
            throw new InvalidDataException();

        int puzzleID = int.Parse(solution.Substring(0, tab), CultureInfo.InvariantCulture);
        var program = Program.Parse(solution.Substring(tab + 1));
        return new LocalSolution { PuzzleID = puzzleID, Program = program };
    }

    LocalSolutions(IFile storage, ICollection<LocalSolution> solutions) {
        this.storage = storage;
        this.solutions = solutions;
    }

    static async Task<List<LocalSolution>> Load(IFile storage) {
        string[]? pending = await storage.ReadLinesAsync().ConfigureAwait(false);

        return pending.Select(ParseSolutionDescriptor).ToList();
    }

    #endregion
}