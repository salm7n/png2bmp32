﻿using System;
using System.Collections.Generic;

namespace png2bmp32
{
   /// <summary>
   /// Event argument class.
   /// </summary>
  class PathParsedEventArgs : EventArgs
   {
      public string Path { get; private set; }

      public PathParsedEventArgs(string path)
      {
         Path = path;
      }
   }

   /// <summary>
   /// …
   /// </summary>
   class ArgumentParser
   {
      public string[] Args { get; private set; }

      /// <summary>
      /// …
      /// </summary>
      /// <param name="args">…</param>
      public ArgumentParser(string[] args)
      {
         Args = args;
      }

      public event EventHandler<PathParsedEventArgs> InputPathParsed;
      public event EventHandler FinishedArgumentParsing;

      /// <summary>
      /// …
      /// </summary>
      /// <param name="e">…</param>
      protected virtual void OnInputPathParsed(PathParsedEventArgs e)
      {
         if (InputPathParsed == null) return;
         InputPathParsed(this, e);
      }

      /// <summary>
      /// …
      /// </summary>
      /// <param name="e">…</param>
      protected virtual void OnFinishedArgumentParsing(EventArgs e)
      {
         if (FinishedArgumentParsing == null) return;
         FinishedArgumentParsing(this, e);
      }

      /// <summary>
      /// …
      /// </summary>
      internal void Parse()
      {
         if (Args.Length != 1) throw new Exception(Properties.Resource.strCmdLnParamCount);

         OnInputPathParsed(new PathParsedEventArgs(Args[0]));
         OnFinishedArgumentParsing(new EventArgs());
      }
   }
}
