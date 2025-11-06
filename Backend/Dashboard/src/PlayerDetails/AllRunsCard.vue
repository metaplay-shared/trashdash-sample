<!-- This file is part of Metaplay SDK which is released under the Metaplay SDK License. -->

<template lang="pug">
meta-list-card(
  title="All Runs"
  :item-list="allRuns"
  :filter-sets="filterSets"
  :sort-options="sortOptions"
  :default-sort-option="1"
  :page-size="10"
  )
  template(#item-card="{ item: run }")
    MListItem(condensed)
      //- Title, top left.
      div(class="tw-text-sm tw-font-semibold")
        span(v-if="run.character") {{ run.character }}
        span(
          v-else
          class="tw-italic tw-text-neutral-400"
          ) No Character
        MBadge(
          class="tw-ml-1"
          shape="pill"
          )
          MAbbreviateNumber(
            :number="run.worldDistance"
            round-down
            )
          span m

      //- Top right.
      template(#top-right)
        MBadge {{ run.themeUsed }}

      //- Bottom left.
      template(#bottom-left)
        div
          span(class="tw-mr-1") Fishbones: {{ run.coinsGained }} |
          span(class="tw-mr-1") Premium: {{ run.premiumGained }} |
          span Score: {{ run.score }}

      //- Bottom right.
      template(#bottom-right)
        MDateTime(:instant="DateTime.fromISO(run.timestamp)")
</template>

<script lang="ts" setup>
import { DateTime } from 'luxon'
import { computed } from 'vue'

import { getSinglePlayerSubscriptionOptions } from '@metaplay/core'
import {
  MetaListCard,
  MetaListFilterSet,
  MetaListFilterOption,
  MetaListSortDirection,
  MetaListSortOption,
} from '@metaplay/meta-ui'
import { MAbbreviateNumber, MBadge, MCard, MListItem, MDateTime } from '@metaplay/meta-ui-next'
import { useSubscription } from '@metaplay/subscriptions'

const props = defineProps<{
  /**
   * Id of the player whose run data we want to show.
   */
  playerId: string
}>()

// Subscribe to the data we need to render this component.
const { data: playerData, refresh: playerRefresh } = useSubscription(() =>
  getSinglePlayerSubscriptionOptions(props.playerId)
)

interface CompletedRun {
  timestamp: string
  character: string
  obstacleTypeHit: string
  themeUsed: string
  coinsGained: number
  premiumGained: number
  score: number
  worldDistance: number
}

const allRuns = computed((): CompletedRun[] => {
  return playerData.value?.model.runHistory || []
})

const filterSets = [
  new MetaListFilterSet('theme', [
    new MetaListFilterOption('Day', (x: any) => x.themeUsed === 'Day'),
    new MetaListFilterOption('Night', (x: any) => x.themeUsed === 'Night'),
    new MetaListFilterOption('Tutorial', (x: any) => x.themeUsed === 'Tutorial'),
  ]),
]

const sortOptions = [
  new MetaListSortOption('Timestamp', 'timestamp', MetaListSortDirection.Ascending),
  new MetaListSortOption('Timestamp', 'timestamp', MetaListSortDirection.Descending),
  new MetaListSortOption('World Distance', 'worldDistance', MetaListSortDirection.Ascending),
  new MetaListSortOption('World Distance', 'worldDistance', MetaListSortDirection.Descending),
  new MetaListSortOption('Coins', 'coins', MetaListSortDirection.Ascending),
  new MetaListSortOption('Coins', 'coins', MetaListSortDirection.Descending),
  new MetaListSortOption('Premium', 'premium', MetaListSortDirection.Ascending),
  new MetaListSortOption('Premium', 'premium', MetaListSortDirection.Descending),
  new MetaListSortOption('Score', 'score', MetaListSortDirection.Ascending),
  new MetaListSortOption('Score', 'score', MetaListSortDirection.Descending),
]
</script>
