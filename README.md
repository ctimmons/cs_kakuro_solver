cs_kakuro_solver
================

A Kakuro puzzle solver written in C#.

Introduction
------------

[Kakuro](http://en.wikipedia.org/wiki/Kakuro) puzzles look kind of like a crossword puzzle, but have a more in common with [Sudoku](http://en.wikipedia.org/wiki/Sudoku) than crosswords.

In the past, I'd tried to write a Sudoku solver, but the naive algorithm I implemented only worked for the easiest puzzles.  Looking for a better algorithm led me to Peter Norvig's elegant [Sudoku solver](http://norvig.com/sudoku.html), written in Python.  I took his idea of using backtracking to solve Sudoku puzzles and applied it to solving Kakuro puzzles.

Build Dependencies
------------------

This project depends on my [Utilities.Core](https://github.com/ctimmons/cs_utilities) library.  Download cs_utilities and include Utilities.Core.csproj as an existing project in KakuroSolver.sln.

Running the Program
-------------------

Running the executable with no parameters will display a short help listing describing how to call the program and its command line switches.

In the source code, this help text can be found in Program.cs.

The Log and Solution Files
--------------------------

A log file and solution file are always created.  By default, their names are simple modifications of the input file name, and the log file only contains simple timing data (unless the /log switch is present).  Different log and output file names can be specified thru the relevant command line switches.

NB:  When run with the /log switch on hard puzzles, the log file can get very large (> 100 MB), and the program will run very slowly (many minutes or even hours will elapse.)

Without the /log switch, the program is CPU-bound.  For example, on my current system (Intel QuadCore Q6600 @ 2.40 GHz), the hardest puzzle included in the repository - ~/sample puzzles/kakuro_11.txt - takes about 15 minutes to solve without the /log switch.

With the /log switch, the program becomes I/O-bound, and runtimes can become very large.  I tried solving kakuro_11.txt with the /log switch.  After only about 10 minutes the log file was over 200 MB and climbing.  I stopped the program before the log file filled up my hard drive.

Algorithm Overview
---------------------

A Kakuro puzzle is a two dimensional grid of cells.

![Annotated Kakuro Puzzle](https://github.com/ctimmons/cs_kakuro_solver/raw/master/images/kakuro.png)

(original image taken from [Wikipedia](http://en.wikipedia.org/wiki/File:Kakuro_black_box.svg).)

Data Structures
---------------

The input file is a plain text file.  The file format is a textual representation of a Kakuro puzzle.  Sample input files are in the repository's ~\Sample Puzzles folder.  Whitespace is not significant in the input file, but it makes the file easier to read.

The sum, value, and peer data structures are in Cell.cs.

- Sum cells contain a "down" sum for columns, and/or a "right" sum for rows.  A sum of 0 is used to represent the blackened squares in a Kakuro puzzle.
- Value cells each contain a single number from 0 to 9.  A value of 0 means the cell hasn't been solved yet.
- The value cells are grouped into "peers", and each group of peers has a sum the value cells must add up to.

After it's read from an input file, the puzzle is parsed into a two-dimensional array of sum and value cells (Kakuro.cs::Load()).  Groups of peers are then constructed from this data (Kakuro.cs::AssignRowAndColumnPeersToValueCells()).

Methods
-------

Before the backtracking algorithm can get to work, each group of row and column peers needs to determine what candidate values are available for placement in the peer group's value cells.  This is calculated by the Utility.cs::GetPartitionsOfSum() method.  It implements an algorithm to construct a list of integers that add up to a given sum.  For example, given a sum of 23 that must fit into 3 value cells, and limiting the partition values to between 1 and 9, the integer partitions are 6, 8, and 9.

To find the candidates for an individual cell, the set intersection of that cell's row and column peer candidates is calculated, minus any value cell values that have already been solved. (See the Candidates property in both the ValueCell and Peers classes in Cell.cs.)

Now that the candidates for each value cell are known, the backtracking algorithm in Kakuro.cs::SolveEx() can begin.  The algorithm is essentially the same as presented on Norvig's [Sudoku Solver page](http://norvig.com/sudoku.html) in the section labeled "Search".

Running the program with the /log switch will fill the log file with entries that illustrate the steps take to solve the puzzle.  The log entries are indented to help visualize the recursion taking place.

Room for Improvement
--------------------

As currently written, this C# app is single-threaded and uses mutable state (i.e. the ValueCell.Value property is modified in the SolveEx() method's recursive case.)

One possible future improvement would be to use multiple threads, each with their own copy of the puzzle, and modifying SolveEx() so it can start its search with an arbitrary value cell.  That way there'd be no need for locking or transactions.  Something to ponder...
