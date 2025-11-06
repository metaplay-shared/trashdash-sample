// This file is part of Metaplay SDK which is released under the Metaplay SDK License.
/* eslint-disable @typescript-eslint/no-unused-vars */
import type { App } from 'vue'

// Import the integration API.
import { OverviewListItem, setGameSpecificInitialization } from '@metaplay/core'

/**
 * This is a Vue 3 plugin function that gets called after the SDK CorePlugin is registered but before the application is mounted.
 * Use this function to register any Vue components or plugins that you want to use to customize the dashboard.
 * @param app The Vue app instance.
 */
export function GameSpecificPlugin(app: App): void {
  // Feel free to add any customization logic here for your game!

  setGameSpecificInitialization((initializationApi) => {
    // Add game-specific code here.
    //#region customize_resources
    initializationApi.addPlayerResources([
      {
        displayName: 'Fishbones',
        getAmount: (playerModel): number => playerModel.playerData.numCoins,
      },
      {
        displayName: 'Premium',
        getAmount: (playerModel): number => playerModel.playerData.numPremium,
      },
    ])
    //#endregion customize_resources

    initializationApi.addPlayerDetailsOverviewListItem(
      OverviewListItem.asString('Themes', (player: any) => {
        const allThemes = player.model.playerData.themes
        return allThemes.join(', ')
      })
    )

    //#region custom_visualization
    initializationApi.addUiComponent(
      'Players/Details/Tab0',
      {
        uniqueId: 'ConsumablesCard',
        vueComponent: async () => await import('./PlayerDetails/ConsumablesCard.vue'),
        width: 'half',
      },
      { position: 'before', targetId: 'Inbox'}
    )
    //#endregion custom_visualization
    initializationApi.addUiComponent(
      'Players/Details/Tab0',
      {
        uniqueId: 'HighScoresCard',
        vueComponent: async () => await import('./PlayerDetails/HighScoresCard.vue'),
        width: 'half',
      },
      { position: 'before', targetId: 'Inbox'}
    )
    initializationApi.addUiComponent(
      'Players/Details/Tab0',
      {
        uniqueId: 'AllRunsCard',
        vueComponent: async () => await import('./PlayerDetails/AllRunsCard.vue'),
        width: 'half',
      },
      { position: 'before', targetId: 'Inbox'}
    )
  })
}
