//----------------------------------------------------------------------------------
//
// IRequest.cs -- The IRequest class.
//
//----------------------------------------------------------------------------------

namespace Mediation.Requests;

//----------------------------------------------------------------------------------
/// <summary>
/// Requests
/// </summary>
/// <typeparam name="TResponse">TResponse</typeparam>
public interface IRequest<out TResponse>;
