namespace Buggy.Model;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IWorkItemQuery
{
    Task<IEnumerable<WorkItem>> RunQuery();
}