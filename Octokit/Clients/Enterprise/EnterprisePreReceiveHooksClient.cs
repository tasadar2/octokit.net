using System.Collections.Generic;
using System.Threading.Tasks;

namespace Octokit
{
    /// <summary>
    /// A client for GitHub's Enterprise Pre-receive Hooks API
    /// </summary>
    /// <remarks>
    /// See the <a href="https://developer.github.com/v3/enterprise-admin/pre_receive_hooks/">Enterprise Pre-receive Hooks API documentation</a> for more information.
    ///</remarks>
    public class EnterprisePreReceiveHooksClient : ApiClient, IEnterprisePreReceiveHooksClient
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EnterprisePreReceiveHooksClient"/>.
        /// </summary>
        /// <param name="apiConnection">An API connection</param>
        public EnterprisePreReceiveHooksClient(IApiConnection apiConnection)
            : base(apiConnection)
        { }

        /// <summary>
        /// Gets all <see cref="PreReceiveHook"/>s.
        /// </summary>
        /// <remarks>
        /// See the <a href="https://developer.github.com/v3/enterprise-admin/pre_receive_hooks/#list-pre-receive-hooks">API documentation</a> for more information.
        /// </remarks>
        /// <exception cref="ApiException">Thrown when a general API error occurs.</exception>
        public Task<IReadOnlyList<PreReceiveHook>> GetAll()
        {
            return GetAll(ApiOptions.None);
        }

        /// <summary>
        /// Gets all <see cref="PreReceiveHook"/>s.
        /// </summary>
        /// <remarks>
        /// See the <a href="https://developer.github.com/v3/enterprise-admin/pre_receive_hooks/#list-pre-receive-hooks">API documentation</a> for more information.
        /// </remarks>
        /// <param name="options">Options for changing the API response</param>
        /// <exception cref="ApiException">Thrown when a general API error occurs.</exception>
        public Task<IReadOnlyList<PreReceiveHook>> GetAll(ApiOptions options)
        {
            var endpoint = ApiUrls.AdminPreReceiveHooks();
            return ApiConnection.GetAll<PreReceiveHook>(endpoint, null, AcceptHeaders.PreReceiveHooksPreview, options);
        }

        /// <summary>
        /// Gets a single <see cref="PreReceiveHook"/>.
        /// </summary>
        /// <remarks>
        /// See the <a href="https://developer.github.com/v3/enterprise-admin/pre_receive_hooks/#get-a-single-pre-receive-hook">API documentation</a> for more information.
        /// </remarks>
        /// <param name="hookId">The id of the pre-receive hook</param>
        /// <exception cref="NotFoundException">Thrown when the specified <paramref name="hookId"/> does not exist.</exception>
        /// <exception cref="ApiException">Thrown when a general API error occurs.</exception>
        public Task<PreReceiveHook> Get(long hookId)
        {
            var endpoint = ApiUrls.AdminPreReceiveHooks(hookId);
            return ApiConnection.Get<PreReceiveHook>(endpoint, null, AcceptHeaders.PreReceiveHooksPreview);
        }

        /// <summary>
        /// Creates a new <see cref="PreReceiveHook"/>.
        /// </summary>
        /// <remarks>
        /// See the <a href="https://developer.github.com/v3/enterprise-admin/pre_receive_hooks/#create-a-pre-receive-hook">API documentation</a> for more information.
        /// </remarks>
        /// <param name="newPreReceiveHook">A description of the pre-receive hook to create</param>
        /// <exception cref="ApiException">Thrown when a general API error occurs.</exception>
        public Task<PreReceiveHook> Create(NewPreReceiveHook newPreReceiveHook)
        {
            Ensure.ArgumentNotNull(newPreReceiveHook, nameof(newPreReceiveHook));

            var endpoint = ApiUrls.AdminPreReceiveHooks();
            return ApiConnection.Post<PreReceiveHook>(endpoint, newPreReceiveHook, AcceptHeaders.PreReceiveHooksPreview);
        }

        /// <summary>
        /// Edits an existing <see cref="PreReceiveHook"/>.
        /// </summary>
        /// <remarks>
        /// See the <a href="https://developer.github.com/v3/enterprise-admin/pre_receive_hooks/#edit-a-pre-receive-hook">API documentation</a> for more information.
        /// </remarks>
        /// <param name="hookId">The id of the pre-receive hook</param>
        /// <param name="updatePreReceiveHook">A description of the pre-receive hook to edit</param>
        /// <exception cref="NotFoundException">Thrown when the specified <paramref name="hookId"/> does not exist.</exception>
        /// <exception cref="ApiException">Thrown when a general API error occurs.</exception>
        public Task<PreReceiveHook> Edit(long hookId, UpdatePreReceiveHook updatePreReceiveHook)
        {
            Ensure.ArgumentNotNull(updatePreReceiveHook, nameof(updatePreReceiveHook));

            var endpoint = ApiUrls.AdminPreReceiveHooks(hookId);
            return ApiConnection.Patch<PreReceiveHook>(endpoint, updatePreReceiveHook, AcceptHeaders.PreReceiveHooksPreview);
        }

        /// <summary>
        /// Deletes an existing <see cref="PreReceiveHook"/>.
        /// </summary>
        /// <remarks>
        /// See the <a href="https://developer.github.com/v3/enterprise-admin/pre_receive_hooks/#delete-a-pre-receive-hook">API documentation</a> for more information.
        /// </remarks>
        /// <param name="hookId">The id of the pre-receive hook</param>
        /// <exception cref="NotFoundException">Thrown when the specified <paramref name="hookId"/> does not exist.</exception>
        /// <exception cref="ApiException">Thrown when a general API error occurs.</exception>
        public Task Delete(long hookId)
        {
            var endpoint = ApiUrls.AdminPreReceiveHooks(hookId);
            return ApiConnection.Delete(endpoint, new object(), AcceptHeaders.PreReceiveHooksPreview);
        }
    }
}
