setWidgetsCustomConfig(
    function () {
        widgetsConfig.crazyTable.dataScript = 'getdata.php';
        widgetsConfig.crazyTable.dataScript = 'getuniconfdata.php';
        widgetsConfig.crazyTable.hierarchyKeepExpandState = true;
        widgetsConfig.crazyTable.hierarchyAutoExpandMaxLevel = -1;

        /*
        // Settings can be assigned globally or to individual crazytables.
        // Repeat the crazytable settings under this key using the crazytable
        // source name as the hash index.
        widgetsConfig.crazyTable.sourceConfig = {};

        // Content for when a CrazyTable cannot initialize
        widgetsConfig.crazyTable.initFailedMsg = '';

        // Script to get the data from, required for CrazyTable to operate
        widgetsConfig.crazyTable.dataScript = '';

        // Hierarchy
            // Expand/collapse can be disabled, hierarchy is then be loaded up
            // in one shot
            widgetsConfig.crazyTable.hierarchyFoldable = true;
            // Whether nodes are clickable or not (the +/- sign is always
            // clickable)
            widgetsConfig.crazyTable.hierarchyNodesClickable = true;
            // Ability to auto collapse sub nodes when collapsing a node
            widgetsConfig.crazyTable.hierarchyKeepExpandState = true;
            // Ability to expand nodes down to a certain level at
            // initialization, -1 for none
            widgetsConfig.crazyTable.hierarchyAutoExpandMaxLevel = -1;

        // Rows
            // Duplicate rows can be disabled (duplicates are then ignored)
            widgetsConfig.crazyTable.rowsDuplicateAllowed = true;
            // Actions on multiple nodes can be enabled
            widgetsConfig.crazyTable.rowsTaggingEnabled = false;

        // Columns
            // Columns order, use empty array for all cols
            widgetsConfig.crazyTable.colsOrder = [];
            // Columns toggling can be enabled
            widgetsConfig.crazyTable.colsTogglingEnabled = false;
            // Make some columns available only in one mode (hierarchy or global
            // sort), use empty array for all cols
            widgetsConfig.crazyTable.colsInHierarchy = [];
            widgetsConfig.crazyTable.colsInGlobalSort = [];

        // Sort
            // Ability to enable sorting
            widgetsConfig.crazyTable.sortEnabled = false;
            // Ability to allow sorting on some columns only, use empty array
            // for all cols
            widgetsConfig.crazyTable.sortCols = [];
            // Restrict sorting mode: 'globalsort', 'hierarchy', or 'both'
            widgetsConfig.crazyTable.sortInMode = 'both';

        // Search
            // Search can be enabled
            widgetsConfig.crazyTable.searchEnabled = false;
            // Restrict searching mode: 'globalsort', 'hierarchy', or 'both'
            widgetsConfig.crazyTable.searchInMode = 'both';
            // Search can use only some cols, use empty array for all cols
            widgetsConfig.crazyTable.searchCols = [];
            // Search autocompletion can be enabled
            widgetsConfig.crazyTable.searchAutoCompleteEnabled = false;
            // Columns used as content for search autocompletion, use empty
            // array for all cols
            widgetsConfig.crazyTable.searchAutoCompleteCols = [];
        */
    }
);
