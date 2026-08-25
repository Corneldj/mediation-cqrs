//----------------------------------------------------------------------------------
//
// PipelineDelegate.cs -- The PipelineDelegate delegate.
//
//----------------------------------------------------------------------------------

namespace Mediation.Requests;

//----------------------------------------------------------------------------------
/// <summary>
/// Represents the continuation of the pipeline, invoking either the next behavior
/// or the terminal handler(s).
/// </summary>
/// <returns>Task</returns>
public delegate Task PipelineDelegate();
