﻿using Lab2.Abstractions;
using Lab2.SpecialFigure;
using Lab2.SpecialFigure.Commands;
using Lab2.SpecialFigure.Commands.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Lab2.GUI
{
    public class MainWindowVM : BaseViewModel
    {
        #region Bindings

        private RelayCommand _undoCmd;
        public RelayCommand UndoCmd
            => _undoCmd ?? (_undoCmd = new RelayCommand(_ => Undo()));

        private ObservableCollection<HistoryCommandVM> _commandsHistory;
        public ObservableCollection<HistoryCommandVM> CommandsHistory
            => _commandsHistory ?? (_commandsHistory = new ObservableCollection<HistoryCommandVM>());

        private FigureVM _figureVM;
        public FigureVM FigureVM => _figureVM ?? (_figureVM = new FigureVM());

        // Macro

        private bool _isRecordingMacros = false;
        public bool IsRecordingMacros
        {
            get => _isRecordingMacros;
            set
            {
                _isRecordingMacros = value;
                NotifyPropChanged(nameof(IsRecordingMacros));
            }
        }

        public int MacrosCount => _chainCommandBuilder.CommandsCount;

        private RelayCommand _startMacrosCmd;
        public RelayCommand StartMacrosCmd
            => _startMacrosCmd ?? (_startMacrosCmd = new RelayCommand(_ => StartMacro()));

        private RelayCommand _applyMacrosCmd;
        public RelayCommand ApplyMacrosCmd
            => _applyMacrosCmd ?? (_applyMacrosCmd = new RelayCommand(_ => ApplyMacro()));

        private RelayCommand _cancelMacrosCmd;
        public RelayCommand CancelMacrosCmd
            => _cancelMacrosCmd ?? (_cancelMacrosCmd = new RelayCommand(_ => CancelMacro()));

        

        // TODO delete
        private RelayCommand _testCmd;
        public RelayCommand TestCmd
            => _testCmd ?? (_testCmd = new RelayCommand(_ => Test()));

        private RelayCommand _test2Cmd;
        public RelayCommand Test2Cmd
            => _test2Cmd ?? (_test2Cmd = new RelayCommand(_ => Test2()));

        #endregion

        #region FigureCommands

        private ICommand _changeFigureTypeCmd;
        public ICommand ChangeTypeCmd => _changeFigureTypeCmd ??
            (_changeFigureTypeCmd = DecorateBoth(new ChangeTypeCommand(FigureVM)));

        private ICommand _changeRadiusCmd;
        public ICommand ChangeRadiusCmd => _changeRadiusCmd ??
            (_changeRadiusCmd = DecorateBoth(new ChangeRadiusCommand(FigureVM)));

        private ICommand _changePenColorCmd;
        public ICommand ChangePenColorCmd => _changePenColorCmd ??
            (_changePenColorCmd = DecorateBoth(new ChangePenColorCommand(FigureVM)));

        private ICommand _changeBrushColorCmd;
        public ICommand ChangeBrushColorCmd => _changeBrushColorCmd ??
            (_changeBrushColorCmd = DecorateBoth(new ChangeBrushColorCommand(FigureVM)));

        private ICommand _changeBorderThicknessCmd;
        public ICommand ChangeBorderThicknessCmd => _changeBorderThicknessCmd ??
            (_changeBorderThicknessCmd = DecorateBoth(new ChangeBorderThicknessCommand(FigureVM)));

        #endregion

        private IChainCommandBuilder _chainCommandBuilder = new ChainCommandBuilder(5);

        public MainWindowVM()
        {

        }

        private ICommand DecorateBoth(IFigureCommand command)
        {
            ICommand snapshoted = DecorateBySnapshotSaver(command);
            return DecorateByRecord(command, snapshoted);
        }

        private ICommand DecorateBySnapshotSaver(IFigureCommand command)
        {
            SnapshotSaver snapshotSaver = new SnapshotSaver(FigureVM, command);
            snapshotSaver.OnSnapshot += OnSnapshot;
            return snapshotSaver;

            void OnSnapshot(object sender, SnapshotEventArgs args)
            {
                CommandsHistory.Add(new HistoryCommandVM(args));
            }
        }

        private ICommand DecorateBySnapshotSaver(ICommand command, string commandName)
        {
            SnapshotSaver snapshotSaver = new SnapshotSaver(FigureVM, command, commandName);
            snapshotSaver.OnSnapshot += OnSnapshot;
            return snapshotSaver;

            void OnSnapshot(object sender, SnapshotEventArgs args)
            {
                CommandsHistory.Add(new HistoryCommandVM(args));
            }
        }

        private ICommand DecorateByRecord(ICommand recordCmd, ICommand executeCmd)
        {
            return new RecordCommand(recordCmd, executeCmd, 
                () => IsRecordingMacros, _chainCommandBuilder);
        }

        private void Undo()
        {
            if (CommandsHistory.Count == 0)
                return;

            HistoryCommandVM historyCommand = CommandsHistory[CommandsHistory.Count - 1];
            historyCommand.Snapshot.Restore();
            CommandsHistory.RemoveAt(CommandsHistory.Count - 1);
        }

        private void StartMacro()
        {
            if (IsRecordingMacros)
                return;
            IsRecordingMacros = true;
        }

        private void ApplyMacro()
        {
            if (MacrosCount > 0)
            {
                ICommand macroCommand = _chainCommandBuilder.Result;
                macroCommand = DecorateBySnapshotSaver(macroCommand, "Macros");
                macroCommand.Execute(null);
                _chainCommandBuilder.Reset();
            }
            IsRecordingMacros = false;
        }

        private void CancelMacro()
        {
            IsRecordingMacros = false;
            _chainCommandBuilder.Reset();
            NotifyPropChanged(nameof(MacrosCount));
        }




        // TODO delete
        private void Test()
        {

        }

        private void Test2()
        {

            
        }



    }
}
