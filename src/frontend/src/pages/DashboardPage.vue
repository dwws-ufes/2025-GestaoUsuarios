<template>
  <q-page class="q-pa-md">
    <div class="row q-col-gutter-md">
      <div class="col-xs-12 col-sm-6 col-md-3">
        <q-card flat bordered class="my-card">
          <q-card-section>
            <div class="text-h6">{{ $t('dashboardPage.activeUsers') }}</div>
            <div class="text-h5 text-bold text-primary">
              {{ commonStore.dashboardStats.activeUsers }}
            </div>
          </q-card-section>
          <q-card-actions align="right">
            <q-btn flat round icon="people" color="primary" @click="goTo('/usuarios')" />
            <q-btn flat :label="$t('dashboardPage.viewUsers')" @click="goTo('/usuarios')" />
          </q-card-actions>
        </q-card>
      </div>

      <div class="col-xs-12 col-sm-6 col-md-3">
        <q-card flat bordered class="my-card">
          <q-card-section>
            <div class="text-h6">{{ $t('dashboardPage.registeredProfiles') }}</div>
            <div class="text-h5 text-bold text-teal">
              {{ commonStore.dashboardStats.totalProfiles }}
            </div>
          </q-card-section>
          <q-card-actions align="right">
            <q-btn flat round icon="badge" color="teal" @click="goTo('/perfis')" />
            <q-btn flat :label="$t('dashboardPage.viewProfiles')" @click="goTo('/perfis')" />
          </q-card-actions>
        </q-card>
      </div>

      <div class="col-xs-12 col-sm-6 col-md-3">
        <q-card flat bordered class="my-card">
          <q-card-section>
            <div class="text-h6">{{ $t('dashboardPage.totalPermissions') }}</div>
            <div class="text-h5 text-bold text-blue-grey-8">
              {{ commonStore.dashboardStats.totalPermissions }}
            </div>
          </q-card-section>
          <q-card-actions align="right">
            <q-btn flat round icon="key" color="blue-grey-8" />
          </q-card-actions>
        </q-card>
      </div>

      <div class="col-xs-12 col-sm-6 col-md-3">
        <q-card flat bordered class="my-card">
          <q-card-section>
            <div class="text-h6">{{ $t('dashboardPage.accessesToday') }}</div>
            <div class="text-h5 text-bold text-indigo">
              {{ commonStore.dashboardStats.accessesToday }}
            </div>
          </q-card-section>
          <q-card-actions align="right">
            <q-btn flat round icon="history" color="indigo" @click="goTo('/acessos')" />
            <q-btn flat :label="$t('dashboardPage.viewHistory')" @click="goTo('/acessos')" />
          </q-card-actions>
        </q-card>
      </div>

      <div class="col-xs-12 col-md-8">
        <q-card flat bordered>
          <q-card-section>
            <div class="row items-center no-wrap">
              <div class="col">
                <div class="text-h6">{{ $t('dashboardPage.dailyAccessesLast7Days') }}</div>
              </div>
              <div class="col-auto">
                <q-btn
                  flat
                  round
                  icon="download"
                  @click="exportDailyAccessesCsv"
                  :disable="!commonStore.dashboardChartData.labels.length"
                >
                  <q-tooltip>{{ $t('dashboardPage.exportChartData') }}</q-tooltip>
                </q-btn>
              </div>
            </div>
          </q-card-section>
          <q-card-section>
            <div
              v-if="
                commonStore.dashboardChartData.labels &&
                commonStore.dashboardChartData.labels.length > 0 &&
                commonStore.dashboardChartData.data &&
                commonStore.dashboardChartData.data.length > 0
              "
            >
              <apexchart
                type="bar"
                height="350"
                :options="chartOptions"
                :series="chartSeries"
              ></apexchart>
            </div>
            <div v-else class="text-center q-py-md text-grey">
              <q-icon name="bar_chart" size="xl" />
              <p>{{ $t('dashboardPage.noRecentAccessData') }}</p>
            </div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-xs-12 col-md-4">
        <q-card flat bordered>
          <q-card-section>
            <div class="row items-center no-wrap">
              <div class="col">
                <div class="text-h6">{{ $t('dashboardPage.recentFailedAccessAttempts') }}</div>
              </div>
              <div class="col-auto">
                <q-btn
                  flat
                  round
                  icon="download"
                  @click="exportFailedAccessesCsv"
                  :disable="!commonStore.dashboardFailedAccesses.length"
                >
                  <q-tooltip>{{ $t('dashboardPage.exportFailedAccesses') }}</q-tooltip>
                </q-btn>
              </div>
            </div>
          </q-card-section>
          <q-card-section>
            <q-list separator>
              <q-item v-for="access in commonStore.dashboardFailedAccesses" :key="access.id">
                <q-item-section>
                  <q-item-label>{{
                    access.usuario?.nome || $t('usersPage.noProfileAssigned')
                  }}</q-item-label>
                  <q-item-label caption>
                    {{ formatDate(access.dataHora) }} - {{ access.ip }}
                  </q-item-label>
                  <q-item-label caption class="text-negative">
                    {{ $t('dashboardPage.failure') }}:
                    {{ access.agente || $t('dashboardPage.columnAgent') }}
                  </q-item-label>
                </q-item-section>
              </q-item>
              <q-item v-if="!commonStore.dashboardFailedAccesses.length">
                <q-item-section class="text-center text-grey">
                  {{ $t('dashboardPage.noRecentFailedAccess') }}
                </q-item-section>
              </q-item>
            </q-list>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <q-inner-loading :showing="commonStore.isDashboardLoading">
      <q-spinner-hourglass color="primary" size="50px" />
    </q-inner-loading>
  </q-page>
</template>

<script setup>
import { useCommonStore } from 'stores/common-store'
import { computed, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import dayjs from 'dayjs'
import 'dayjs/locale/pt-br'
import 'dayjs/locale/en'
dayjs.locale('pt-br')

const commonStore = useCommonStore()
const $q = useQuasar()
const { t, locale } = useI18n()
const router = useRouter()

// --- Computed Properties para ApexCharts (mantidas como estão) ---
const chartOptions = computed(() => {
  const primaryColor = $q.palette?.primary || '#1976D2'

  return {
    chart: {
      type: 'bar',
      height: 350,
      toolbar: {
        show: false,
      },
    },
    plotOptions: {
      bar: {
        horizontal: false,
        columnWidth: '55%',
        endingShape: 'rounded',
      },
    },
    dataLabels: {
      enabled: false,
    },
    stroke: {
      show: true,
      width: 2,
      colors: ['transparent'],
    },
    xaxis: {
      categories: commonStore.dashboardChartData.labels,
      labels: {
        style: {
          colors: $q.dark.isActive ? '#fff' : '#000',
        },
      },
    },
    yaxis: {
      title: {
        text: t('dashboardPage.accessCount'),
        style: {
          color: $q.dark.isActive ? '#fff' : '#000',
        },
      },
      labels: {
        formatter: function (value) {
          return Math.floor(value)
        },
        style: {
          colors: $q.dark.isActive ? '#fff' : '#000',
        },
      },
    },
    fill: {
      opacity: 1,
      colors: [primaryColor],
    },
    tooltip: {
      y: {
        formatter: function (val) {
          return `${val} ${t('dashboardPage.accesses')}`
        },
      },
    },
    grid: {
      show: true,
      borderColor: $q.dark.isActive ? '#333' : '#e0e0e0',
      strokeDashArray: 0,
      position: 'back',
      xaxis: {
        lines: {
          show: false,
        },
      },
      yaxis: {
        lines: {
          show: true,
        },
      },
    },
  }
})

const chartSeries = computed(() => {
  return [
    {
      name: t('dashboardPage.accesses'),
      data: commonStore.dashboardChartData.data,
    },
  ]
})

// --- Métodos (mantidos como estão) ---

const goTo = (path) => {
  router.push(path)
}

const carregarDashboardData = async () => {
  try {
    await commonStore.fetchDashboardData()
  } catch (error) {
    $q.notify({
      color: 'negative',
      message: commonStore.dashboardErrorMessage || t('errors.loadDashboardFailed'),
      icon: 'error',
      timeout: 3000,
    })
    console.error('Erro ao carregar dados do dashboard:', error)
  }
}

const formatDate = (dateString) => {
  return dayjs(dateString).locale(locale.value).format('DD/MM/YYYY HH:mm:ss')
}

// Função para gerar e baixar CSV (mantida como está)
const exportToCsv = (filename, rows) => {
  if (!rows || rows.length === 0) {
    $q.notify({
      color: 'info',
      message: t('dashboardPage.noDataToExport'),
      icon: 'info',
      timeout: 2000,
    })
    return
  }

  const headers = Object.keys(rows[0])
    .map((key) => `"${key}"`)
    .join(',')

  const csvRows = rows.map((row) => {
    return Object.values(row)
      .map((value) => {
        let formattedValue = value === null || value === undefined ? '' : String(value)
        formattedValue = formattedValue.replace(/"/g, '""')
        return `"${formattedValue}"`
      })
      .join(',')
  })

  const csvContent = [headers, ...csvRows].join('\n')

  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  if (link.download !== undefined) {
    const url = URL.createObjectURL(blob)
    link.setAttribute('href', url)
    link.setAttribute('download', filename)
    link.style.visibility = 'hidden'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
    $q.notify({
      color: 'positive',
      message: t('dashboardPage.exportSuccess', { filename: filename }),
      icon: 'check_circle',
      timeout: 2000,
    })
  } else {
    $q.notify({
      color: 'negative',
      message: t('dashboardPage.exportFailed'),
      icon: 'error',
      timeout: 3000,
    })
  }
}

const exportDailyAccessesCsv = () => {
  const chartData = commonStore.dashboardChartData
  if (!chartData.labels || !chartData.data || chartData.labels.length === 0) {
    exportToCsv('acessos_diarios.csv', [])
    return
  }

  const rows = chartData.labels.map((label, index) => ({
    [t('dashboardPage.columnDate')]: label,
    [t('dashboardPage.accessCount')]: chartData.data[index],
  }))
  exportToCsv('acessos_diarios.csv', rows)
}

const exportFailedAccessesCsv = () => {
  const failedAccesses = commonStore.dashboardFailedAccesses
  if (!failedAccesses || failedAccesses.length === 0) {
    exportToCsv('acessos_falhos.csv', [])
    return
  }

  const rows = failedAccesses.map((access) => ({
    [t('dashboardPage.columnDateTime')]: formatDate(access.dataHora),
    [t('dashboardPage.columnUser')]: access.usuario?.nome || t('usersPage.noProfileAssigned'),
    [t('dashboardPage.columnIP')]: access.ip,
    [t('dashboardPage.columnAgent')]: access.agente || '-',
    [t('dashboardPage.columnError')]: access.mensagemErro || '-',
  }))
  exportToCsv('acessos_falhos.csv', rows)
}

// --- Lifecycle Hook ---
onMounted(() => {
  carregarDashboardData()
})
</script>

<style scoped>
/* Estilos específicos para esta página, se necessário */
.my-card {
  transition: all 0.3s ease-in-out;
  cursor: pointer;
}

.my-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
}
</style>
