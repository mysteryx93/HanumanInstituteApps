using System;
using System.Collections.Generic;
using HanumanInstitute.CommonWpf;
using Xunit;

namespace HanumanInstitute.CommonWpfApp.Tests.CommonUI
{
    public class SelectableListTests
    {
        [Fact]
        public void Constructor_NoParam_HasRightDefaultValues()
        {
            var model = new SelectableList<TestListItem>();

            Assert.NotNull(model.List);
            Assert.Equal(-1, model.SelectedIndex);
            Assert.Null(model.SelectedItem);
            Assert.False(model.HasSelection);
        }

        [Fact]
        public void SelectedIndex_Set_HasExpectedSelectionValues()
        {
            var model = new SelectableList<TestListItem>();
            var newItem = new TestListItem();
            model.List.Add(newItem);

            model.SelectedIndex = 0;

            Assert.Equal(0, model.SelectedIndex);
            Assert.Equal(newItem, model.SelectedItem);
            Assert.True(model.HasSelection);
        }

        [Fact]
        public void SelectedIndex_SetAndClear_ResetSelectionValues()
        {
            var model = new SelectableList<TestListItem>();
            var newItem = new TestListItem();
            model.List.Add(newItem);

            model.SelectedIndex = 0;
            model.SelectedIndex = -1;

            Assert.Equal(-1, model.SelectedIndex);
            Assert.Null(model.SelectedItem);
            Assert.False(model.HasSelection);
        }

        //[Theory]
        //[InlineData(1, 0, false, 0)]
        //[InlineData(1, 0, true, 0)]
        //[InlineData(2, 1, false, 1)]
        //[InlineData(2, 1, true, 1)]
        //[InlineData(2, 2, false, -1)]
        //[InlineData(2, 2, true, 1)]
        //[InlineData(2, -2, false, -1)]
        //[InlineData(2, -2, true, 0)]
        //public void Select_ForceSelect_HasExpectedSelectedIndex(int count, int selIndex, bool force, int expected)
        //{
        //    var model = new SelectableList<TestListItem>();
        //    for (int i = 0; i < count; i++)
        //    {
        //        model.List.Add(new TestListItem());
        //    }

        //    model.ForceSelect(selIndex, force);

        //    Assert.Equal(expected, model.SelectedIndex);
        //}

        [Fact]
        public void SelectedIndex_Set_TriggersPropertyChanged()
        {
            var model = new SelectableList<TestListItem>();
            model.List.Add(new TestListItem());
            bool selectedIndexChanged = false;
            bool selectedItemChanged = false;
            bool hasSelectionChanged = false;
            model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(model.SelectedIndex))
                {
                    selectedIndexChanged = true;
                }
                else if (e.PropertyName == nameof(model.SelectedItem))
                {
                    selectedItemChanged = true;
                }
                else if (e.PropertyName == nameof(model.HasSelection))
                {
                    hasSelectionChanged = true;
                }
            };

            model.SelectedIndex = 0;

            Assert.True(selectedIndexChanged);
            Assert.True(selectedItemChanged);
            Assert.True(hasSelectionChanged);
        }

        [Fact]
        public void SelectedIndex_ReplaceItemAndSetSameIndex_TriggersSelectedItemChanged()
        {
            var model = new SelectableList<TestListItem>();
            model.List.Add(new TestListItem());
            bool selectedItemChanged = false;
            model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(model.SelectedItem))
                {
                    selectedItemChanged = true;
                }
            };
            model.SelectedIndex = 0;

            model.List.Clear();
            model.List.Add(new TestListItem());
            model.SelectedIndex = 0;

            Assert.True(selectedItemChanged);
        }

        [Fact]
        public void ReplaceAll_NewList_ListItemsReplaced()
        {
            var model = new SelectableList<TestListItem>();
            model.List.Add(new TestListItem());
            var newList = new List<TestListItem>()
            {
                new TestListItem(),
                new TestListItem(),
                new TestListItem()
            };

            model.ReplaceAll(newList);

            Assert.Equal(newList, model.List);
        }

        private class TestListItem
        {
        }
    }
}
