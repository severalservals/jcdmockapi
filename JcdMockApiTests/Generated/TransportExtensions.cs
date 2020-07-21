// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace JcdMockApi
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for Transport.
    /// </summary>
    public static partial class TransportExtensions
    {
            /// <summary>
            /// Get me the wombats. All of them.
            /// </summary>
            /// <remarks>
            /// Just like the summary. If you want all the wombats, look here. If you want
            /// fewer than that, try something else.
            /// </remarks>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            public static string Wombats(this ITransport operations)
            {
                return operations.WombatsAsync().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get me the wombats. All of them.
            /// </summary>
            /// <remarks>
            /// Just like the summary. If you want all the wombats, look here. If you want
            /// fewer than that, try something else.
            /// </remarks>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<string> WombatsAsync(this ITransport operations, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.WombatsWithHttpMessagesAsync(null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Create brand new wombats
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='body'>
            /// The information about the wombats we want to create
            /// </param>
            public static void Wombats1(this ITransport operations, string body = default(string))
            {
                operations.Wombats1Async(body).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Create brand new wombats
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='body'>
            /// The information about the wombats we want to create
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task Wombats1Async(this ITransport operations, string body = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.Wombats1WithHttpMessagesAsync(body, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <summary>
            /// Get me a wombat. A specific one.
            /// </summary>
            /// <remarks>
            /// Just one wombat, but you have to know exactly the one you want.
            /// </remarks>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// The id of the wombat we want.
            /// </param>
            public static string Wombats2(this ITransport operations, int id)
            {
                return operations.Wombats2Async(id).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get me a wombat. A specific one.
            /// </summary>
            /// <remarks>
            /// Just one wombat, but you have to know exactly the one you want.
            /// </remarks>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// The id of the wombat we want.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<string> Wombats2Async(this ITransport operations, int id, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.Wombats2WithHttpMessagesAsync(id, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Update a wombat
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// The id of the wombat to update
            /// </param>
            /// <param name='body'>
            /// The value to use in updating the wombat
            /// </param>
            public static void Wombats3(this ITransport operations, int id, string body = default(string))
            {
                operations.Wombats3Async(id, body).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Update a wombat
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// The id of the wombat to update
            /// </param>
            /// <param name='body'>
            /// The value to use in updating the wombat
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task Wombats3Async(this ITransport operations, int id, string body = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.Wombats3WithHttpMessagesAsync(id, body, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <summary>
            /// Delete a wombat
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// The id of the wombat to delete
            /// </param>
            public static void Wombats4(this ITransport operations, int id)
            {
                operations.Wombats4Async(id).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Delete a wombat
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// The id of the wombat to delete
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task Wombats4Async(this ITransport operations, int id, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.Wombats4WithHttpMessagesAsync(id, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

    }
}
