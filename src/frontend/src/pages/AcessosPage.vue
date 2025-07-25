<!-- /src/pages/AcessosPage.vue-->
<template>
  <q-page class="q-pa-md">
    <div class="row items-center justify-between q-mb-md">
      <div class="text-h5">{{ t('accessesPage.title') }}</div>
      <div class="row items-center">
        <q-input
          dense
          debounce="300"
          v-model="filtroAcessos"
          :placeholder="t('accessesPage.searchPlaceholder')"
          class="q-ml-sm"
        >
          <template #append>
            <q-icon name="search" />
          </template>
        </q-input>
        <q-btn
          flat
          round
          icon="download"
          class="q-ml-md"
          @click="exportAccessesCsv"
          :disable="!acessosFiltrados.length"
        >
          <q-tooltip>{{ t('accessesPage.exportAccesses') }}</q-tooltip>
        </q-btn>
      </div>
    </div>

    <q-table
      :rows="acessosFiltrados"
      :columns="translatedColumns"
      row-key="id"
      flat
      bordered
      dense
      :loading="commonStore.isAccessesLoading"
      :no-data-label="t('accessesPage.noDataAvailable')"
      v-model:pagination="pagination"
      :rows-per-page-options="[10, 20, 50, 100]"
    >
      <template #body-cell-sucesso="props">
        <q-td align="center">
          <q-badge
            :color="props.row.sucesso ? 'green' : 'red'"
            :label="props.row.sucesso ? t('accessesPage.success') : t('accessesPage.failure')"
            transparent
            class="text-uppercase"
          />
        </q-td>
      </template>

      <template #loading>
        <q-inner-loading showing color="primary" />
      </template>
    </q-table>
  </q-page>
</template>

<script setup>
import { onMounted, computed, ref } from 'vue'
import { useQuasar } from 'quasar' // Importar useQuasar
import dayjs from 'dayjs'
import { useI18n } from 'vue-i18n'
import { useCommonStore } from 'stores/common-store'

const { t } = useI18n()
const $q = useQuasar() // Instanciar useQuasar
const commonStore = useCommonStore()

const filtroAcessos = ref('')

const pagination = ref({
  sortBy: 'dataHora',
  descending: true,
  page: 1,
  rowsPerPage: 10,
})

const translatedColumns = computed(() => {
  return [
    {
      name: 'usuario',
      label: t('accessesPage.userColumn'),
      field: (row) => (row.usuario ? row.usuario.nome : 'N/A'),
      sortable: true,
    },
    {
      name: 'dataHora',
      label: t('accessesPage.dateTimeColumn'),
      field: 'dataHora',
      format: (val) => dayjs(val).format('DD/MM/YYYY HH:mm'),
      sortable: true,
    },
    { name: 'ip', label: t('accessesPage.ipColumn'), field: 'ip', sortable: true },
    { name: 'agente', label: t('accessesPage.agentColumn'), field: 'agente', sortable: false },
    {
      name: 'sucesso',
      label: t('accessesPage.resultColumn'),
      field: 'sucesso',
      sortable: true,
      align: 'center',
    },
  ]
})

const acessosFiltrados = computed(() => {
  if (!filtroAcessos.value) {
    return commonStore.accessesData
  }
  const termo = filtroAcessos.value.toLowerCase()
  return commonStore.accessesData.filter(
    (acesso) =>
      (acesso.usuario?.nome && acesso.usuario.nome.toLowerCase().includes(termo)) ||
      (acesso.ip && acesso.ip.toLowerCase().includes(termo)) ||
      (acesso.agente && acesso.agente.toLowerCase().includes(termo)) ||
      (acesso.sucesso
        ? t('accessesPage.success').toLowerCase().includes(termo)
        : t('accessesPage.failure').toLowerCase().includes(termo)),
  )
})

// Função de formatação de data (reutilizada do Dashboard)
const formatDate = (dateString) => {
  // Certifique-se de que o locale seja o mesmo que está sendo usado no Day.js globalmente
  // Ou importe e defina o locale aqui se for diferente da global
  // dayjs.locale(locale.value) // Se 'locale' estiver disponível no escopo
  return dayjs(dateString).format('DD/MM/YYYY HH:mm:ss')
}

// Função utilitária para exportar CSV (reutilizada do Dashboard)
const exportToCsv = (filename, rows) => {
  if (!rows || rows.length === 0) {
    $q.notify({
      color: 'info',
      message: t('dashboardPage.noDataToExport'), // Reutilizando tradução do dashboard
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
      message: t('dashboardPage.exportSuccess', { filename: filename }), // Reutilizando tradução
      icon: 'check_circle',
      timeout: 2000,
    })
  } else {
    $q.notify({
      color: 'negative',
      message: t('dashboardPage.exportFailed'), // Reutilizando tradução
      icon: 'error',
      timeout: 3000,
    })
  }
}

// Função específica para exportar os acessos
const exportAccessesCsv = () => {
  const accessesToExport = acessosFiltrados.value // Exporta os dados filtrados
  if (!accessesToExport || accessesToExport.length === 0) {
    exportToCsv('historico_acessos.csv', [])
    return
  }

  const rows = accessesToExport.map((access) => ({
    [t('accessesPage.dateTimeColumn')]: formatDate(access.dataHora),
    [t('accessesPage.userColumn')]: access.usuario?.nome || t('usersPage.noProfileAssigned'), // Ajuste para texto padrão
    [t('accessesPage.ipColumn')]: access.ip,
    [t('accessesPage.agentColumn')]: access.agente || '-',
    [t('accessesPage.resultColumn')]: access.sucesso
      ? t('accessesPage.success')
      : t('accessesPage.failure'),
  }))
  exportToCsv('historico_acessos.csv', rows)
}

onMounted(async () => {
  await commonStore.fetchAccesses()
})
</script>

<style lang="scss" scoped>
/* Estilos existentes ou adicionais podem vir aqui */
</style>
