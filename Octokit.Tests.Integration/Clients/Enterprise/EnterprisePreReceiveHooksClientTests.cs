using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using Octokit.Tests.Integration;
using Xunit;

public class EnterprisePreReceiveHooksClientTests
{
    public class TheCtor
    {
        [Fact]
        public void EnsuresNonNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new EnterprisePreReceiveHooksClient(null));
        }
    }

    public class TheGetAllMethod : IDisposable
    {
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveHooksClient _preReceiveHooksClient;
        private readonly List<PreReceiveHook> _preReceiveHooks;

        public TheGetAllMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveHooksClient = _githubEnterprise.Enterprise.PreReceiveHooks;

            _preReceiveHooks = new List<PreReceiveHook>();
            for (var count = 0; count < 3; count++)
            {
                var newPreReceiveHook = new NewPreReceiveHook(Helper.MakeNameWithTimestamp("hook"), "octokit/octokit.net", Helper.MakeNameWithTimestamp("script"), 1);
                _preReceiveHooks.Add(_preReceiveHooksClient.Create(newPreReceiveHook).Result);
            }
        }

        [IntegrationTest]
        public async Task ReturnsPreReceiveHooks()
        {
            var preReceiveHooks = await _preReceiveHooksClient.GetAll();

            Assert.NotEmpty(preReceiveHooks);
        }

        [IntegrationTest]
        public async Task ReturnsCorrectCountOfPreReceiveHooksWithoutStart()
        {
            var options = new ApiOptions
            {
                PageSize = 1,
                PageCount = 1
            };

            var preReceiveHooks = await _preReceiveHooksClient.GetAll(options);

            Assert.Equal(1, preReceiveHooks.Count);
        }

        [IntegrationTest]
        public async Task ReturnsCorrectCountOfPreReceiveHooksWithStart()
        {
            var options = new ApiOptions
            {
                PageSize = 1,
                PageCount = 1,
                StartPage = 2
            };

            var preReceiveHooks = await _preReceiveHooksClient.GetAll(options);

            Assert.Equal(1, preReceiveHooks.Count);
        }

        [IntegrationTest]
        public async Task ReturnsDistinctResultsBasedOnStartPage()
        {
            var startOptions = new ApiOptions
            {
                PageSize = 1,
                PageCount = 1
            };

            var firstPage = await _preReceiveHooksClient.GetAll(startOptions);

            var skipStartOptions = new ApiOptions
            {
                PageSize = 1,
                PageCount = 1,
                StartPage = 2
            };

            var secondPage = await _preReceiveHooksClient.GetAll(skipStartOptions);

            Assert.NotEqual(firstPage[0].Id, secondPage[0].Id);
        }

        public void Dispose()
        {
            foreach (var preReceiveHook in _preReceiveHooks)
            {
                EnterpriseHelper.DeletePreReceiveHook(_githubEnterprise.Connection, preReceiveHook);
            }
        }
    }

    public class TheGetMethod : IDisposable
    {
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveHooksClient _preReceiveHooksClient;
        private readonly NewPreReceiveHook _expectedPreReceiveHook;
        private readonly PreReceiveHook _preReceiveHook;

        public TheGetMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveHooksClient = _githubEnterprise.Enterprise.PreReceiveHooks;

            _expectedPreReceiveHook = new NewPreReceiveHook(Helper.MakeNameWithTimestamp("hook"), "octokit/octokit.net", Helper.MakeNameWithTimestamp("script"), 1)
            {
                AllowDownstreamConfiguration = true,
                Enforcement = PreReceiveHookEnforcement.Testing,
            };
            _preReceiveHook = _preReceiveHooksClient.Create(_expectedPreReceiveHook).Result;
        }

        [IntegrationTest]
        public async Task ReturnsName()
        {
            var preReceiveHook = await _preReceiveHooksClient.Get(_preReceiveHook.Id);

            Assert.NotNull(preReceiveHook);
            Assert.Equal(_expectedPreReceiveHook.Name, preReceiveHook.Name);
        }

        [IntegrationTest]
        public async Task ReturnsScript()
        {
            var preReceiveHook = await _preReceiveHooksClient.Get(_preReceiveHook.Id);

            Assert.NotNull(preReceiveHook);
            Assert.Equal(_expectedPreReceiveHook.Script, preReceiveHook.Script);
        }

        [IntegrationTest]
        public async Task ReturnsRepository()
        {
            var preReceiveHook = await _preReceiveHooksClient.Get(_preReceiveHook.Id);

            Assert.NotNull(preReceiveHook);
            Assert.NotNull(preReceiveHook.ScriptRepository);
            Assert.Equal(_expectedPreReceiveHook.ScriptRepository.FullName, preReceiveHook.ScriptRepository.FullName);
        }

        [IntegrationTest]
        public async Task ReturnsEnvironment()
        {
            var preReceiveHook = await _preReceiveHooksClient.Get(_preReceiveHook.Id);

            Assert.NotNull(preReceiveHook);
            Assert.NotNull(preReceiveHook.Environment);
            Assert.Equal(_expectedPreReceiveHook.Environment.Id, preReceiveHook.Environment.Id);
        }

        [IntegrationTest]
        public async Task ReturnsAllowDownstreamConfiguration()
        {
            var preReceiveHook = await _preReceiveHooksClient.Get(_preReceiveHook.Id);

            Assert.NotNull(preReceiveHook);
            Assert.Equal(_expectedPreReceiveHook.AllowDownstreamConfiguration, preReceiveHook.AllowDownstreamConfiguration);
        }

        [IntegrationTest]
        public async Task ReturnsEnforcement()
        {
            var preReceiveHook = await _preReceiveHooksClient.Get(_preReceiveHook.Id);

            Assert.NotNull(preReceiveHook);
            Assert.Equal(_expectedPreReceiveHook.Enforcement.Value, preReceiveHook.Enforcement);
        }

        [IntegrationTest]
        public async Task NoHookExists()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _preReceiveHooksClient.Get(-1));
        }

        public void Dispose()
        {
            EnterpriseHelper.DeletePreReceiveHook(_githubEnterprise.Connection, _preReceiveHook);
        }
    }

    public class TheCreateMethod
    {
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveHooksClient _preReceiveHooksClient;

        public TheCreateMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveHooksClient = _githubEnterprise.Enterprise.PreReceiveHooks;
        }

        [Fact]
        public async Task CanCreatePreReceiveHook()
        {
            PreReceiveHook preReceiveHook = null;
            try
            {
                var newPreReceiveHook = new NewPreReceiveHook(Helper.MakeNameWithTimestamp("hook"), "octokit/octokit.net", Helper.MakeNameWithTimestamp("script"), 1);
                preReceiveHook = await _preReceiveHooksClient.Create(newPreReceiveHook);

                Assert.NotNull(preReceiveHook);
                Assert.Equal(newPreReceiveHook.Name, preReceiveHook.Name);
                Assert.Equal(newPreReceiveHook.Script, preReceiveHook.Script);
                Assert.Equal(newPreReceiveHook.ScriptRepository.FullName, preReceiveHook.ScriptRepository.FullName);
                Assert.Equal(newPreReceiveHook.Environment.Id, preReceiveHook.Environment.Id);
            }
            finally
            {
                //Cleanup
                EnterpriseHelper.DeletePreReceiveHook(_githubEnterprise.Connection, preReceiveHook);
            }
        }

        [Fact]
        public async Task CannotCreateWhenRepoDoesNotExist()
        {
            var newPreReceiveHook = new NewPreReceiveHook(Helper.MakeNameWithTimestamp("hook"), "doesntExist/repo", Helper.MakeNameWithTimestamp("script"), 1);
            await Assert.ThrowsAsync<ApiValidationException>(async () => await _preReceiveHooksClient.Create(newPreReceiveHook));
        }

        [Fact]
        public async Task CannotCreateWhenEnvironmentDoesNotExist()
        {
            var newPreReceiveHook = new NewPreReceiveHook(Helper.MakeNameWithTimestamp("hook"), "octokit/octokit.net", Helper.MakeNameWithTimestamp("script"), -1);
            await Assert.ThrowsAsync<ApiValidationException>(async () => await _preReceiveHooksClient.Create(newPreReceiveHook));
        }

        [Fact]
        public async Task CannotCreateWithSameName()
        {
            PreReceiveHook preReceiveHook = null;
            try
            {
                var newPreReceiveHook = new NewPreReceiveHook(Helper.MakeNameWithTimestamp("hook"), "octokit/octokit.net", Helper.MakeNameWithTimestamp("script"), 1);
                preReceiveHook = await _preReceiveHooksClient.Create(newPreReceiveHook);

                newPreReceiveHook.Script = Helper.MakeNameWithTimestamp("script");
                await Assert.ThrowsAsync<ApiValidationException>(async () => await _preReceiveHooksClient.Create(newPreReceiveHook));
            }
            finally
            {
                //Cleanup
                EnterpriseHelper.DeletePreReceiveHook(_githubEnterprise.Connection, preReceiveHook);
            }
        }

        [Fact]
        public async Task CannotCreateWithSameScript()
        {
            PreReceiveHook preReceiveHook = null;
            try
            {
                var newPreReceiveHook = new NewPreReceiveHook(Helper.MakeNameWithTimestamp("hook"), "octokit/octokit.net", Helper.MakeNameWithTimestamp("script"), 1);
                preReceiveHook = await _preReceiveHooksClient.Create(newPreReceiveHook);

                newPreReceiveHook.Name = Helper.MakeNameWithTimestamp("hook");
                await Assert.ThrowsAsync<ApiValidationException>(async () => await _preReceiveHooksClient.Create(newPreReceiveHook));
            }
            finally
            {
                //Cleanup
                EnterpriseHelper.DeletePreReceiveHook(_githubEnterprise.Connection, preReceiveHook);
            }
        }
    }

    public class TheEditMethod : IDisposable
    {
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveHooksClient _preReceiveHooksClient;
        private readonly PreReceiveHook _preReceiveHook;

        public TheEditMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveHooksClient = _githubEnterprise.Enterprise.PreReceiveHooks;

            var newPreReceiveHook = new NewPreReceiveHook(Helper.MakeNameWithTimestamp("hook"), "octokit/octokit.net", Helper.MakeNameWithTimestamp("script"), 1);
            _preReceiveHook = _preReceiveHooksClient.Create(newPreReceiveHook).Result;
        }

        [IntegrationTest]
        public async Task CanChangeName()
        {
            var updatePreReceiveHook = new UpdatePreReceiveHook
            {
                Name = Helper.MakeNameWithTimestamp("hook")
            };

            var updatedPreReceiveHook = await _preReceiveHooksClient.Edit(_preReceiveHook.Id, updatePreReceiveHook);

            Assert.Equal(_preReceiveHook.Id, updatedPreReceiveHook.Id);
            Assert.Equal(updatePreReceiveHook.Name, updatedPreReceiveHook.Name);
        }

        [IntegrationTest]
        public async Task CanChangeScript()
        {
            var updatePreReceiveHook = new UpdatePreReceiveHook
            {
                Script = Helper.MakeNameWithTimestamp("script")
            };

            var updatedPreReceiveHook = await _preReceiveHooksClient.Edit(_preReceiveHook.Id, updatePreReceiveHook);

            Assert.Equal(_preReceiveHook.Id, updatedPreReceiveHook.Id);
            Assert.Equal(updatePreReceiveHook.Script, updatedPreReceiveHook.Script);
        }

        public void Dispose()
        {
            EnterpriseHelper.DeletePreReceiveHook(_githubEnterprise.Connection, _preReceiveHook);
        }
    }

    public class TheDeleteMethod
    {
        private readonly IEnterprisePreReceiveHooksClient _preReceiveHooksClient;

        public TheDeleteMethod()
        {
            var githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveHooksClient = githubEnterprise.Enterprise.PreReceiveHooks;
        }

        [IntegrationTest]
        public async Task CanDelete()
        {
            var newPreReceiveHook = new NewPreReceiveHook(Helper.MakeNameWithTimestamp("hook"), "octokit/octokit.net", Helper.MakeNameWithTimestamp("script"), 1);
            var preReceiveHook = await _preReceiveHooksClient.Create(newPreReceiveHook);

            await _preReceiveHooksClient.Delete(preReceiveHook.Id);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _preReceiveHooksClient.Get(preReceiveHook.Id));
        }

        [IntegrationTest]
        public async Task CannotDeleteWhenHookDoesNotExist()
        {
            await Assert.ThrowsAsync<NotFoundException>(async () => await _preReceiveHooksClient.Delete(-1));
        }
    }
}
