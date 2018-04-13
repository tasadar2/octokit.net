﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using Octokit.Tests.Integration;
using Xunit;

public class EnterprisePreReceiveEnvironmentClientTests
{
    public class TheCtor
    {
        [Fact]
        public void EnsuresNonNullArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ReleasesClient(null));
        }
    }

    public class TheGetAllMethod : IDisposable
    {
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveEnvironmentsClient _preReceiveEnvironmentsClient;
        private readonly List<PreReceiveEnvironment> _preReceiveEnvironments;

        public TheGetAllMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveEnvironmentsClient = _githubEnterprise.Enterprise.PreReceiveEnvironments;

            _preReceiveEnvironments = new List<PreReceiveEnvironment>();
            for (var count = 0; count < 5; count++)
            {
                var newPreReceiveEnvironment = new NewPreReceiveEnvironment(Helper.MakeNameWithTimestamp("pre-receive"), "https://example.com/foo.zip");
                _preReceiveEnvironments.Add(_preReceiveEnvironmentsClient.Create(newPreReceiveEnvironment).Result);
            }
        }

        [IntegrationTest]
        public async Task ReturnsPreReceiveEnvironments()
        {
            var preReceiveEnvironments = await _preReceiveEnvironmentsClient.GetAll();

            Assert.NotEmpty(preReceiveEnvironments);
        }

        [IntegrationTest]
        public async Task ReturnsCorrectCountOfPreReceiveEnvironmentsWithoutStart()
        {
            var options = new ApiOptions
            {
                PageSize = 1,
                PageCount = 1
            };

            var preReceiveEnvironments = await _preReceiveEnvironmentsClient.GetAll(options);

            Assert.Equal(1, preReceiveEnvironments.Count);
        }

        [IntegrationTest]
        public async Task ReturnsCorrectCountOfPreReceiveEnvironmentsWithStart()
        {
            var options = new ApiOptions
            {
                PageSize = 1,
                PageCount = 1,
                StartPage = 2
            };

            var preReceiveEnvironments = await _preReceiveEnvironmentsClient.GetAll(options);

            Assert.Equal(1, preReceiveEnvironments.Count);
        }

        [IntegrationTest]
        public async Task ReturnsDistinctResultsBasedOnStartPage()
        {
            var startOptions = new ApiOptions
            {
                PageSize = 1,
                PageCount = 1
            };

            var firstPage = await _preReceiveEnvironmentsClient.GetAll(startOptions);

            var skipStartOptions = new ApiOptions
            {
                PageSize = 1,
                PageCount = 1,
                StartPage = 2
            };

            var secondPage = await _preReceiveEnvironmentsClient.GetAll(skipStartOptions);

            Assert.NotEqual(firstPage[0].Id, secondPage[0].Id);
        }

        public void Dispose()
        {
            foreach (var preReceiveEnvironment in _preReceiveEnvironments)
            {
                EnterpriseHelper.DeletePreReceiveEnvironment(_githubEnterprise.Connection, preReceiveEnvironment);
            }
        }
    }

    public class TheGetMethod : IDisposable
    {
        private readonly string _preReceiveEnvironmentName;
        private readonly string _preReceiveEnvironmentUrl;
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveEnvironmentsClient _preReceiveEnvironmentsClient;
        private readonly PreReceiveEnvironment _preReceiveEnvironment;

        public TheGetMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveEnvironmentsClient = _githubEnterprise.Enterprise.PreReceiveEnvironments;

            _preReceiveEnvironmentName = Helper.MakeNameWithTimestamp("pre-receive");
            _preReceiveEnvironmentUrl = "https://example.com/foo.zip";
            var newPreReceiveEnvironment = new NewPreReceiveEnvironment(_preReceiveEnvironmentName, _preReceiveEnvironmentUrl);
            _preReceiveEnvironment = _preReceiveEnvironmentsClient.Create(newPreReceiveEnvironment).Result;
        }

        [IntegrationTest]
        public async Task ReturnsName()
        {
            var preReceiveEnvironment = await _preReceiveEnvironmentsClient.Get(_preReceiveEnvironment.Id);

            Assert.NotNull(preReceiveEnvironment);
            Assert.Equal(_preReceiveEnvironmentName, preReceiveEnvironment.Name);
        }

        [IntegrationTest]
        public async Task ReturnsImageUrl()
        {
            var preReceiveEnvironment = await _preReceiveEnvironmentsClient.Get(_preReceiveEnvironment.Id);

            Assert.NotNull(preReceiveEnvironment);
            Assert.Equal(_preReceiveEnvironmentUrl, preReceiveEnvironment.ImageUrl);
        }

        [IntegrationTest]
        public async Task NoEnvironmentExists()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _preReceiveEnvironmentsClient.Get(-1));
        }

        public void Dispose()
        {
            EnterpriseHelper.DeletePreReceiveEnvironment(_githubEnterprise.Connection, _preReceiveEnvironment);
        }
    }

    public class TheCreateMethod
    {
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveEnvironmentsClient _preReceiveEnvironmentsClient;

        public TheCreateMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveEnvironmentsClient = _githubEnterprise.Enterprise.PreReceiveEnvironments;
        }

        [Fact]
        public async Task CanCreatePreReceiveEnvironment()
        {
            PreReceiveEnvironment preReceiveEnvironment = null;
            try
            {
                var newPreReceiveEnvironment = new NewPreReceiveEnvironment(Helper.MakeNameWithTimestamp("pre-receive"), "https://example.com/foo.zip");

                preReceiveEnvironment = await _preReceiveEnvironmentsClient.Create(newPreReceiveEnvironment);

                Assert.NotNull(preReceiveEnvironment);
                Assert.Equal(newPreReceiveEnvironment.Name, preReceiveEnvironment.Name);
                Assert.Equal(newPreReceiveEnvironment.ImageUrl, preReceiveEnvironment.ImageUrl);
            }
            finally
            {
                //Cleanup
                EnterpriseHelper.DeletePreReceiveEnvironment(_githubEnterprise.Connection, preReceiveEnvironment);
            }
        }

        [Fact]
        public async Task CannotCreateWithSameName()
        {
            var newPreReceiveEnvironment = new NewPreReceiveEnvironment("default", "https://example.com/foo.zip");

            await Assert.ThrowsAsync<ApiValidationException>(() => _preReceiveEnvironmentsClient.Create(newPreReceiveEnvironment));
        }
    }

    public class TheEditMethod : IDisposable
    {
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveEnvironmentsClient _preReceiveEnvironmentsClient;
        private readonly PreReceiveEnvironment _preReceiveEnvironment;

        public TheEditMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveEnvironmentsClient = _githubEnterprise.Enterprise.PreReceiveEnvironments;

            var newPreReceiveEnvironment = new NewPreReceiveEnvironment(Helper.MakeNameWithTimestamp("pre-receive"), "https://example.com/foo.zip");
            _preReceiveEnvironment = _preReceiveEnvironmentsClient.Create(newPreReceiveEnvironment).Result;
            EnterpriseHelper.WaitForPreReceiveEnvironmentToComplete(_githubEnterprise.Connection, _preReceiveEnvironment);
        }

        [IntegrationTest]
        public async Task CanChangeNameOfPreReceiveEnvironment()
        {
            var updatePreReceiveEnvironment = _preReceiveEnvironment.ToUpdate();
            updatePreReceiveEnvironment.Name = Helper.MakeNameWithTimestamp("pre-receive");

            var updatedPreReceiveEnvironment = await _preReceiveEnvironmentsClient.Edit(_preReceiveEnvironment.Id, updatePreReceiveEnvironment);

            Assert.Equal(_preReceiveEnvironment.Id, updatedPreReceiveEnvironment.Id);
            Assert.Equal(updatePreReceiveEnvironment.Name, updatedPreReceiveEnvironment.Name);
        }

        [IntegrationTest]
        public async Task CanChangeImageUrlOfPreReceiveEnvironment()
        {
            var updatePreReceiveEnvironment = _preReceiveEnvironment.ToUpdate();
            updatePreReceiveEnvironment.ImageUrl = "https://example.com/bar.zip";

            var updatedPreReceiveEnvironment = await _preReceiveEnvironmentsClient.Edit(_preReceiveEnvironment.Id, updatePreReceiveEnvironment);

            Assert.Equal(_preReceiveEnvironment.Id, updatedPreReceiveEnvironment.Id);
            Assert.Equal(updatePreReceiveEnvironment.ImageUrl, updatedPreReceiveEnvironment.ImageUrl);
        }

        public void Dispose()
        {
            EnterpriseHelper.DeletePreReceiveEnvironment(_githubEnterprise.Connection, _preReceiveEnvironment);
        }
    }

    public class TheDeleteMethod
    {
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveEnvironmentsClient _preReceiveEnvironmentsClient;

        public TheDeleteMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveEnvironmentsClient = _githubEnterprise.Enterprise.PreReceiveEnvironments;
        }

        [IntegrationTest]
        public async Task CanDeletePreReceiveEnvironment()
        {
            var newPreReceiveEnvironment = new NewPreReceiveEnvironment(Helper.MakeNameWithTimestamp("pre-receive"), "https://example.com/foo.zip");
            var preReceiveEnvironment = await _preReceiveEnvironmentsClient.Create(newPreReceiveEnvironment);
            EnterpriseHelper.WaitForPreReceiveEnvironmentToComplete(_githubEnterprise.Connection, preReceiveEnvironment);

            await _preReceiveEnvironmentsClient.Delete(preReceiveEnvironment.Id);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _preReceiveEnvironmentsClient.Get(preReceiveEnvironment.Id));
        }

        [IntegrationTest]
        public async Task CannotDeleteWhenNoEnvironmentExists()
        {
            await Assert.ThrowsAsync<NotFoundException>(async () => await _preReceiveEnvironmentsClient.Delete(-1));
        }
    }

    public class TheDownloadStatusMethod
    {
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveEnvironmentsClient _preReceiveEnvironmentsClient;

        public TheDownloadStatusMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveEnvironmentsClient = _githubEnterprise.Enterprise.PreReceiveEnvironments;
        }

        [IntegrationTest]
        public async Task CanGetDownloadStatus()
        {
            PreReceiveEnvironment preReceiveEnvironment = null;
            try
            {
                var newPreReceiveEnvironment = new NewPreReceiveEnvironment(Helper.MakeNameWithTimestamp("pre-receive"), "https://example.com/foo.zip");
                preReceiveEnvironment = await _preReceiveEnvironmentsClient.Create(newPreReceiveEnvironment);

                var downloadStatus = await _preReceiveEnvironmentsClient.DownloadStatus(preReceiveEnvironment.Id);

                Assert.NotNull(downloadStatus);
            }
            finally
            {
                //Cleanup
                EnterpriseHelper.DeletePreReceiveEnvironment(_githubEnterprise.Connection, preReceiveEnvironment);
            }
        }

        [IntegrationTest]
        public async Task CannotGetDownloadStatusWhenNoEnvironmentExists()
        {
            await Assert.ThrowsAsync<NotFoundException>(async () => await _preReceiveEnvironmentsClient.DownloadStatus(-1));
        }
    }

    public class TheTriggerDownloadMethod
    {
        private readonly IGitHubClient _githubEnterprise;
        private readonly IEnterprisePreReceiveEnvironmentsClient _preReceiveEnvironmentsClient;

        public TheTriggerDownloadMethod()
        {
            _githubEnterprise = EnterpriseHelper.GetAuthenticatedClient();
            _preReceiveEnvironmentsClient = _githubEnterprise.Enterprise.PreReceiveEnvironments;
        }

        [IntegrationTest]
        public async Task CanTriggerDownload()
        {
            PreReceiveEnvironment preReceiveEnvironment = null;
            try
            {
                var newPreReceiveEnvironment = new NewPreReceiveEnvironment(Helper.MakeNameWithTimestamp("pre-receive"), "https://example.com/foo.zip");
                preReceiveEnvironment = await _preReceiveEnvironmentsClient.Create(newPreReceiveEnvironment);

                var downloadStatus = await _preReceiveEnvironmentsClient.DownloadStatus(preReceiveEnvironment.Id);

                Assert.NotNull(downloadStatus);
            }
            finally
            {
                //Cleanup
                EnterpriseHelper.DeletePreReceiveEnvironment(_githubEnterprise.Connection, preReceiveEnvironment);
            }
        }

        [IntegrationTest]
        public async Task CannotTriggerDownloadWhenNoEnvironmentExists()
        {
            await Assert.ThrowsAsync<NotFoundException>(async () => await _preReceiveEnvironmentsClient.DownloadStatus(-1));
        }
    }
}
