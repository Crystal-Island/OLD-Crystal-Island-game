using System;
using UnityEngine;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    /// <summary>
    /// Defines all possible game states.
    /// </summary>
    public class PolymoneyGameFlow : GameFlow
    {
        [Flags]
        [StateFlags]
        public enum FlowStates
        {
            INTRO_WORLD = 0x1,
            CHARACTER_GENERATION = 0x2,
            PLAYER_INTRODUCTION = 0x4,
            BEGIN_MONTH = 0x8,
            PLAYER_EVENTS = 0x10,
            PLAYER_TRADE = 0x20,
            END_MONTH = 0x40,
            END = 0x80,
            FAIRYDUST_SHARING = 0x200,
            GAMEOVER = 0x400,
            Q_INTRODUCTION = 0x800,
            Q_MARKET = 0x1000,
            MOVEMENT_TUTO = 0x2000,
            SUMMARY = 0x4000,
            QUITTING = 0x8000,
            END_MONTH_DISPLAY = 0x20000,
            
        }
        protected override IFlowPhase createFlow()
        {
            IFlowPhase intro = new FlowPhase((int)FlowStates.INTRO_WORLD, "INTRO_WORLD");
            IFlowPhase chargen = new FlowPhase((int)FlowStates.CHARACTER_GENERATION, "CHARACTER_GENERATION");
            IFlowPhase movementTuto = new FlowPhase((int)FlowStates.MOVEMENT_TUTO, "MOVEMENT_TUTO");
            IFlowPhase playerIntro = new FlowPhase((int)FlowStates.PLAYER_INTRODUCTION, "PLAYER_INTRODUCTION");
            IFlowPhase beginMonth = new FlowPhase((int)FlowStates.BEGIN_MONTH, "BEGIN_MONTH");
            IFlowPhase playerEvents = new FlowPhase((int)FlowStates.PLAYER_EVENTS, "PLAYER_EVENTS");
            IFlowPhase playerTrade = new FlowPhase((int)FlowStates.PLAYER_TRADE, "PLAYER_TRADE");
            IFlowPhase endMonth = new FlowPhase((int)FlowStates.END_MONTH, "END_MONTH");
            IFlowPhase end = new FlowPhase((int)FlowStates.END, "END");
            IFlowPhase fairySharing = new FlowPhase((int)FlowStates.FAIRYDUST_SHARING, "FAIRYDUST_SHARING");
            IFlowPhase gameOver = new FlowPhase((int)FlowStates.GAMEOVER, "GAMEOVER");
            IFlowPhase qIntroduction = new FlowPhase((int)FlowStates.Q_INTRODUCTION, "Q_INTRODUCTION");
            IFlowPhase qMarket = new FlowPhase((int)FlowStates.Q_MARKET, "Q_MARKET");
            IFlowPhase summary = new FlowPhase((int)FlowStates.SUMMARY, "SUMMARY");
            IFlowPhase quitting = new FlowPhase((int)FlowStates.QUITTING, "QUITTING");
            IFlowPhase endMonthDisplay = new FlowPhase((int)FlowStates.END_MONTH_DISPLAY, "END_MONTH_DISPLAY");


            intro.add(playerIntro);
            playerIntro.add(movementTuto);
            movementTuto.add(beginMonth);
            beginMonth.add(playerEvents);
            playerEvents.add(playerTrade);
            playerTrade.add(endMonth);
            endMonth.add(endMonthDisplay);
            endMonthDisplay.add(gameOver);
            endMonthDisplay.add(end);
            endMonthDisplay.add(qIntroduction);
            endMonthDisplay.add(fairySharing);
            fairySharing.add(beginMonth);
            qIntroduction.add(qMarket);
            qMarket.add(beginMonth);

            gameOver.add(summary);
            end.add(summary);
            summary.add(quitting);

            RootLogger.Info(this, "The initial state will be '{0}'", intro);
            return intro;
        }
    }
}
