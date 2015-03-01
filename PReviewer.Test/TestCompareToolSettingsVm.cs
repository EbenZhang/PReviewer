using System;
using System.Collections.Generic;
using System.Linq;
using ExtendedCL;
using NSubstitute;
using NUnit.Framework;
using PReviewer.Domain;
using PReviewer.Model;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestCompareToolSettingsVm
    {
        private List<CompareToolSettings> _compareTools;
        private CompareToolSettingsContainer _compareToolsContainer;
        private ICompareToolSettingsPersist _compareToolSettingsPersist;
        private CompareToolSettingsVm _viewModel;

        [SetUp]
        public void Setup()
        {
            _compareTools = new List<CompareToolSettings>
            {
                new CompareToolSettings
                {
                    Name = "Beyond Compare3",
                    ExePath = "dummyPath"
                },
                new CompareToolSettings
                {
                    Name = "KDiff3",
                    ExePath = "dummyPath"
                }
            };

            _compareToolsContainer = new CompareToolSettingsContainer(_compareTools, -1);

            _compareToolSettingsPersist = Substitute.For<ICompareToolSettingsPersist>();
            _compareToolSettingsPersist.Load().Returns(_compareToolsContainer);
            _viewModel = new CompareToolSettingsVm(_compareToolSettingsPersist);
        }

        [Test]
        public async void BusyStatusChanges_WhenLoadCompareTools()
        {
            var count = 0;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == PropertyName.Get((CompareToolSettingsVm x) => x.IsProcessing))
                {
                    count++;
                }
            };

            await _viewModel.Load();

            Assert.That(count, Is.EqualTo(2), "Busy status should change twice");
            Assert.False(_viewModel.IsProcessing);
        }

        [Test]
#pragma warning disable 1998
        public async void GivenAFailureWhenLoad_BusyStatusShouldBeReset()
#pragma warning restore 1998
        {
            _compareToolSettingsPersist.When(x => x.Load()).Do(x => { throw new Exception(); });
            Assert.Throws<Exception>(async () => await _viewModel.Load());
            Assert.False(_viewModel.IsProcessing);
        }

        [Test]
        public async void CanGetCompareTools()
        {
            await _viewModel.Load();
            CollectionAssert.AreEqual(_compareTools, _viewModel.CompareTools);
        }

        [Test]
        public async void CanSaveSettings()
        {
            _compareToolsContainer.CurrentActiveIndex = 1;
            await _viewModel.Load();

            _compareToolSettingsPersist.When(x => x.Save(Arg.Any<CompareToolSettingsContainer>())).Do(x =>
            {
                var recievedConainter = x.Args()[0] as CompareToolSettingsContainer;
                CollectionAssert.AreEqual(_viewModel.CompareTools, recievedConainter.GetCompareTools());
                Assert.That(recievedConainter.CurrentActiveIndex, Is.EqualTo(_compareToolsContainer.CurrentActiveIndex));
            });

            await _viewModel.Save();

            _compareToolSettingsPersist.Received(1).Save(Arg.Any<CompareToolSettingsContainer>());
        }

        [Test]
        public async void BusyStatusChanges_WhenSave()
        {
            _compareToolsContainer.CurrentActiveIndex = 1;
            await _viewModel.Load();
            var count = 0;
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == PropertyName.Get((CompareToolSettingsVm x) => x.IsProcessing))
                {
                    count++;
                }
            };
            await _viewModel.Save();

            Assert.That(count, Is.EqualTo(2), "Busy status should change twice");
            Assert.False(_viewModel.IsProcessing);
        }

        [Test]
        public async void GivenAFailureWhenSave_BusyStatusShouldReset()
        {
            await _viewModel.Load();
            _compareToolSettingsPersist.When(x => x.Save(Arg.Any<CompareToolSettingsContainer>()))
                .Do(x => { throw new Exception(); });
            Assert.Throws<Exception>(async () => await _viewModel.Save());
            Assert.False(_viewModel.IsProcessing);
        }

        [Test]
        public void ShouldHaveDefaultEmptyCompareTool()
        {
            Assert.NotNull(_viewModel.SelectedComapreToolSettings);
        }

        [Test]
        public async void CanSaveNewCompareToolSetting()
        {
            _viewModel.CompareTools.Assign(_compareTools);
            Assert.NotNull(_viewModel.SelectedComapreToolSettings);
            CollectionAssert.DoesNotContain(_compareTools, _viewModel.SelectedComapreToolSettings);

            _viewModel.SelectedComapreToolSettings.ExePath = "DummyPath";
            _viewModel.SelectedComapreToolSettings.Name = "DummyName";

            _compareToolSettingsPersist
                .When(x => x.Save(Arg.Any<CompareToolSettingsContainer>())).Do(
                    x =>
                    {
                        var expected = new List<CompareToolSettings>(_compareTools);
                        expected.Insert(0, _viewModel.SelectedComapreToolSettings);

                        var actual = x.Args()[0] as CompareToolSettingsContainer;
                        CollectionAssert.AreEqual(expected, actual.GetCompareTools());
                        Assert.That(actual.CurrentActiveIndex, Is.EqualTo(0));
                    });

            await _viewModel.Save();
        }

        [Test]
        public async void GivenANewSettingsAlreadyExists_ShouldUpdateIt()
        {
            _compareToolsContainer.CurrentActiveIndex = 1;
            await _viewModel.Load();
            var orgSize = _compareTools.Count;

            _viewModel.PrepareNewTool();

            Assert.NotNull(_viewModel.SelectedComapreToolSettings);
            CollectionAssert.DoesNotContain(_compareTools, _viewModel.SelectedComapreToolSettings);

            const string newExePath = @"DummyPath";
            const string newParam = "newparam";
            _viewModel.SelectedComapreToolSettings.ExePath = newExePath;
            _viewModel.SelectedComapreToolSettings.Parameters = newParam;
            _viewModel.SelectedComapreToolSettings.Name = _compareTools.First().Name;

            _compareToolSettingsPersist
                .When(x => x.Save(Arg.Any<CompareToolSettingsContainer>())).Do(
                    x =>
                    {
                        var actual = x.Args()[0] as CompareToolSettingsContainer;
                        var newTools = actual.GetCompareTools();
                        Assert.That(actual.CurrentActiveIndex, Is.Not.EqualTo(-1));
                        Assert.That(newTools.Count(), Is.EqualTo(orgSize), "Should just update, so no new tool added.");

                        var updated = newTools.ElementAt(actual.CurrentActiveIndex);
                        Assert.That(updated.ExePath, Is.EqualTo(newExePath));
                        Assert.That(updated.Parameters, Is.EqualTo(newParam));
                        Assert.That(updated.Name, Is.EqualTo(_compareTools.First().Name));
                    });

            await _viewModel.Save();

            Assert.That(_viewModel.CompareTools.Count, Is.EqualTo(orgSize), "No new setting should be added.");
            Assert.That(_viewModel.SelectedComapreToolSettings.ExePath, Is.EqualTo(newExePath),
                "Should update to new exe path");
            Assert.That(_viewModel.SelectedComapreToolSettings.Parameters, Is.EqualTo(newParam),
                "Should update to new parameters");
        }

        [Test]
        public async void ShouldSelectTheActiveAfterLoaded()
        {
            _compareToolsContainer.CurrentActiveIndex = 1;
            await _viewModel.Load();
            Assert.That(_viewModel.SelectedComapreToolSettings,
                Is.EqualTo(_viewModel.CompareTools[_compareToolsContainer.CurrentActiveIndex]));

            _compareToolsContainer.CurrentActiveIndex = -1;
            await _viewModel.Load();
            Assert.That(_viewModel.SelectedComapreToolSettings, Is.EqualTo(null));
        }

        [Test]
        public void CanPrepareNewTool()
        {
            _viewModel.CompareTools.Assign(_compareTools);
            _viewModel.SelectedComapreToolSettings = _compareTools.First();
            _viewModel.PrepareNewTool();
            Assert.NotNull(_viewModel.SelectedComapreToolSettings);
            Assert.IsNullOrEmpty(_viewModel.SelectedComapreToolSettings.Name);
            CollectionAssert.Contains(_viewModel.CompareTools, _viewModel.SelectedComapreToolSettings);
        }

        [Test]
        public async void ShouldExcludeEmptyEntries()
        {
            //// Load
            var orgActiveIndex = _compareToolsContainer.CurrentActiveIndex = 1;
            await _viewModel.Load();
            Assert.That(_viewModel.SelectedComapreToolSettings, Is.EqualTo(_compareTools.ElementAt(orgActiveIndex)));

            //// Prepare new entry
            _viewModel.PrepareNewTool();
            Assert.IsNullOrEmpty(_viewModel.SelectedComapreToolSettings.Name);
            CollectionAssert.Contains(_viewModel.CompareTools, _viewModel.SelectedComapreToolSettings);
            var newEmptyEntry = _viewModel.SelectedComapreToolSettings;

            _compareToolSettingsPersist.When(x => x.Save(Arg.Any<CompareToolSettingsContainer>())).Do(x =>
            {
                var actual = x.Args()[0] as CompareToolSettingsContainer;
                CollectionAssert.AreEqual(_compareTools, actual.GetCompareTools());
                Assert.That(actual.CurrentActiveIndex, Is.EqualTo(orgActiveIndex),
                    "should not change the active index if new entry is empty");
            });

            //// save
            await _viewModel.Save();

            CollectionAssert.DoesNotContain(_viewModel.CompareTools, newEmptyEntry,
                "The new empty entry should be removed");
            Assert.That(_viewModel.SelectedComapreToolSettings, Is.EqualTo(_compareTools.ElementAt(orgActiveIndex)));
        }

        [Test]
        public async void GivenAChangeToTheActiveSelection_TheHasChangesMethodShouldReturnTrue()
        {
            var orgActiveIndex = _compareToolsContainer.CurrentActiveIndex = 1;
            await _viewModel.Load();
            Assert.False(_viewModel.HasChanges(), "Shouldn't has changes initially.");
            _viewModel.SelectedComapreToolSettings = _viewModel.CompareTools.First();
            Assert.True(_viewModel.HasChanges());
        }

        [Test]
        public async void GivenANewEmptySettingAdded_TheHasChangesMethodShouldReturnFalse()
        {
            var orgActiveIndex = _compareToolsContainer.CurrentActiveIndex = 1;
            await _viewModel.Load();
            Assert.False(_viewModel.HasChanges(), "Shouldn't has changes initially.");
            _viewModel.PrepareNewTool();
            Assert.False(_viewModel.HasChanges());
        }

        [Test]
        public async void GivenANewNonEmptySettingAdded_TheHasChangesMethodShouldReturnTrue()
        {
            var orgActiveIndex = _compareToolsContainer.CurrentActiveIndex = 1;
            await _viewModel.Load();
            Assert.False(_viewModel.HasChanges(), "Shouldn't has changes initially.");
            _viewModel.PrepareNewTool();
            _viewModel.SelectedComapreToolSettings.Name = "test";
            _viewModel.SelectedComapreToolSettings.ExePath = "testpath";
            Assert.True(_viewModel.HasChanges());
        }

        [Test]
        public async void AllInvalidSettingsShouldBeExcluded()
        {
            await _viewModel.Load();
            _viewModel.PrepareNewTool();
            _viewModel.PrepareNewTool();
            _viewModel.PrepareNewTool();
            await _viewModel.Save();
            _compareToolSettingsPersist.Received(1)
                .Save(
                    Arg.Is<CompareToolSettingsContainer>(x =>
                        x.GetCompareTools().SequenceEqual(_compareTools) &&
                        x.CurrentActiveIndex == _compareToolsContainer.CurrentActiveIndex));
        }
    }
}