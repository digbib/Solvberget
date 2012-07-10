﻿(function () {
    "use strict";

    var appViewState = Windows.UI.ViewManagement.ApplicationViewState;
    var binding = WinJS.Binding;
    var nav = WinJS.Navigation;
    var ui = WinJS.UI;
    var utils = WinJS.Utilities;

    ui.Pages.define("/pages/lists/libraryLists.html", {

        /// <field type="WinJS.Binding.List" />
        //items: null,
        /// <field type="Object" />
        group: null,
        itemSelectionIndex: -1,

        // This function checks if the list and details columns should be displayed
        // on separate pages instead of side-by-side.
        isSingleColumn: function () {
            var viewState = Windows.UI.ViewManagement.ApplicationView.value;
            return (viewState === appViewState.snapped || viewState === appViewState.fullScreenPortrait);
        },

        // This function is called whenever a user navigates to this page. It
        // populates the page elements with the app's data.
        ready: function (element, options) {

            var listViewForLists = element.querySelector(".listOfLists").winControl;
            var listViewForListContent = element.querySelector(".listOfListContent").winControl;

            //Setup the ListDataSource
            var listDataSource = new DataSources.List.ListDataSource();

            ////Setup an (empty) ListContentDataSource
            //var listContentDataSource = new DataSources.List.ListContentDataSource([]);

            // Store information about the group and selection that this page will
            // display.
            this.group = (options && options.groupKey) ? Data.resolveGroupReference(options.groupKey) : Data.groups.getAt(0);
            //this.items = null;
            this.itemSelectionIndex = (options && "selectedIndex" in options) ? options.selectedIndex : -1;

            //Set page header
            element.querySelector("header[role=banner] .pagetitle").textContent = this.group.title;


            // Set up the listViewForLists.
            listViewForLists.itemDataSource = listDataSource;
            listViewForLists.itemTemplate = element.querySelector(".listListTemplate");
            listViewForLists.onselectionchanged = this.listOfListsSelectionChanged.bind(this);
            listViewForLists.layout = new ui.ListLayout();
            
            // Set up the listViewForLists.
            listViewForListContent.itemTemplate = element.querySelector(".listContentTemplate");
            listViewForListContent.layout = new ui.ListLayout();

           
            //Set the List of content to list 0
            //listViewForListContent.itemDataSource
                
            this.updateVisibility();
            if (this.isSingleColumn()) {
                if (this.itemSelectionIndex >= 0) {
                    // For single-column detail view, load the article.
                    binding.processAll(element.querySelector(".articlesection"), options.item);
                }
            } else {
                if (nav.canGoBack && nav.history.backStack[nav.history.backStack.length - 1].location === "/pages/lists/libraryLists.html") {
                    // Clean up the backstack to handle a user snapping, navigating
                    // away, unsnapping, and then returning to this page.
                    nav.history.backStack.pop();
                }
                // If this page has a selectionIndex, make that selection
                // appear in the listViewForLists.
                listViewForLists.selection.set(Math.max(this.itemSelectionIndex, 0));
            }


        },

        listOfListsSelectionChanged: function (args) {
            var listViewForLists = document.body.querySelector(".listOfLists").winControl;
            var details;
            var that = this;
            // By default, the selection is restriced to a single item.
            listViewForLists.selection.getItems().done(function updateDetails(items) {
                if (items.length > 0) {
                    that.itemSelectionIndex = items[0].index;
                    if (that.isSingleColumn()) {

                        // If snapped or portrait, navigate to a new page containing the
                        // selected item's details.
                        nav.navigate("/pages/lists/libraryLists.html", { groupKey: that.group.key, selectedIndex: that.itemSelectionIndex, item: items[0].data });

                    } else {
                        
                        // If fullscreen or filled, update the details column with new data.
                        details = document.querySelector(".articlesection");
                        binding.processAll(details, items[0].data);
                        details.scrollTop = 0;

                    }
                }
            });
        },

        unload: function () {
            //this.items.dispose();
        },

        // This function updates the page layout in response to viewState changes.
        updateLayout: function (element, viewState, lastViewState) {
            /// <param name="element" domElement="true" />
            /// <param name="viewState" value="Windows.UI.ViewManagement.ApplicationViewState" />
            /// <param name="lastViewState" value="Windows.UI.ViewManagement.ApplicationViewState" />

            var listViewForLists = element.querySelector(".listOfLists").winControl;
            var firstVisible = listViewForLists.indexOfFirstVisible;
            this.updateVisibility();

            var handler = function (e) {
                listViewForLists.removeEventListener("contentanimating", handler, false);
                e.preventDefault();
            }

            if (this.isSingleColumn()) {
                listViewForLists.selection.clear();
                if (this.itemSelectionIndex >= 0) {
                    // If the app has snapped into a single-column detail view,
                    // add the single-column list view to the backstack.
                    nav.history.current.state = {
                        groupKey: this.group.key,
                        selectedIndex: this.itemSelectionIndex
                    };
                    nav.history.backStack.push({
                        location: "/pages/lists/libraryLists.html",
                        state: { groupKey: this.group.key }
                    });
                    element.querySelector(".articlesection").focus();
                } else {
                    listViewForLists.addEventListener("contentanimating", handler, false);
                    listViewForLists.indexOfFirstVisible = firstVisible;
                    listViewForLists.forceLayout();
                }
            } else {
                // If the app has unsnapped into the two-column view, remove any
                // splitPage instances that got added to the backstack.
                if (nav.canGoBack && nav.history.backStack[nav.history.backStack.length - 1].location === "/pages/lists/libraryLists.html") {
                    nav.history.backStack.pop();
                }
                if (viewState !== lastViewState) {
                    listViewForLists.addEventListener("contentanimating", handler, false);
                    listViewForLists.indexOfFirstVisible = firstVisible;
                    listViewForLists.forceLayout();
                }

                listViewForLists.selection.set(this.itemSelectionIndex >= 0 ? this.itemSelectionIndex : Math.max(firstVisible, 0));
            }
        },

        // This function toggles visibility of the two columns based on the current
        // view state and item selection.
        updateVisibility: function () {
            var oldPrimary = document.querySelector(".primarycolumn");
            if (oldPrimary) {
                utils.removeClass(oldPrimary, "primarycolumn");
            }
            if (this.isSingleColumn()) {
                if (this.itemSelectionIndex >= 0) {
                    utils.addClass(document.querySelector(".articlesection"), "primarycolumn");
                    document.querySelector(".articlesection").focus();
                } else {
                    utils.addClass(document.querySelector(".itemlistsection"), "primarycolumn");
                    document.querySelector(".listOfLists").focus();
                }
            } else {
                document.querySelector(".listOfLists").focus();
            }
        }

    });
})();