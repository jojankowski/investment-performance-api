<script setup>
import { ref } from 'vue'

// There's no endpoint that lists users. The demo users are listed here instead.
const users = [1, 2, 3, 4, 5]

const userId = ref(null)
const investments = ref([])
const details = ref(null)
const error = ref('')

async function getJson(url) {
    const response = await fetch(url)
    if (!response.ok) {
        throw new Error(await errorMessage(response))
    }
    return response.json()
}

// The API returns errors as problem details JSON. Anything else means the request never reached it,
// for example when the API isn't running and the dev server's proxy fails.
async function errorMessage(response) {
    const isJson = response.headers.get('content-type')?.includes('json')
    const problem = isJson ? await response.json() : null
    return problem?.title ?? `Couldn't reach the API (status ${response.status}). Is it running?`
}

async function loadInvestments() {
    error.value = ''
    investments.value = []
    details.value = null
    try {
        investments.value = await getJson(`/api/users/${userId.value}/investments`)
    } catch (e) {
        error.value = e.message
    }
}

async function loadDetails(investmentId) {
    error.value = ''
    try {
        details.value = await getJson(`/api/users/${userId.value}/investments/${investmentId}`)
    } catch (e) {
        error.value = e.message
    }
}

// Totals are shown in cents. Per-share prices keep up to the 4 decimal places the API returns.
const money = (amount) => amount.toLocaleString('en-US', { style: 'currency', currency: 'USD' })
const pricePerShare = (amount) => amount.toLocaleString('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 4 })
</script>

<template>
    <main class="card">
        <h1>Investments</h1>

        <label class="picker">
            User
            <select v-model="userId" @change="loadInvestments">
                <option :value="null" disabled>Choose a user</option>
                <option v-for="id in users" :key="id" :value="id">User {{ id }}</option>
            </select>
        </label>

        <p v-if="error" class="error">{{ error }}</p>
        <p v-else-if="userId && investments.length === 0" class="muted">No investments.</p>

        <ul class="investments">
            <li v-for="investment in investments" :key="investment.id">
                <button
                    type="button"
                    :class="{ selected: details?.id === investment.id }"
                    @click="loadDetails(investment.id)">
                    {{ investment.name }}
                </button>
            </li>
        </ul>

        <section v-if="details" class="details">
            <h2>{{ details.name }} <span class="muted">{{ details.symbol }}</span></h2>
            <table>
                <tbody>
                    <tr><th>Shares</th><td>{{ details.shares }}</td></tr>
                    <tr><th>Cost basis per share</th><td>{{ pricePerShare(details.costBasisPerShare) }}</td></tr>
                    <tr><th>Current price</th><td>{{ pricePerShare(details.currentPrice) }}</td></tr>
                    <tr><th>Current value</th><td>{{ money(details.currentValue) }}</td></tr>
                    <tr><th>Term</th><td>{{ details.term }}</td></tr>
                    <tr>
                        <th>Total gain/loss</th>
                        <td :class="details.totalGainLoss < 0 ? 'loss' : 'gain'">{{ money(details.totalGainLoss) }}</td>
                    </tr>
                </tbody>
            </table>
        </section>
    </main>
</template>

<style>
body {
    margin: 0;
    padding: 48px 16px;
    background: #f4f5f7;
    font-family: system-ui, sans-serif;
    color: #1f2328;
}

.card {
    max-width: 560px;
    margin: 0 auto;
    padding: 28px 32px;
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

h1 {
    margin: 0 0 20px;
    font-size: 1.6rem;
}

h2 {
    margin: 0 0 12px;
    font-size: 1.2rem;
}

.picker {
    display: flex;
    align-items: center;
    gap: 10px;
    font-weight: 600;
}

select {
    font: inherit;
    font-weight: normal;
    padding: 8px 10px;
    border: 1px solid #ccd0d5;
    border-radius: 8px;
    background: #fff;
}

select:focus {
    outline: 2px solid #3b6fd8;
    outline-offset: 1px;
}

.investments {
    list-style: none;
    margin: 20px 0 0;
    padding: 0;
}

.investments button {
    width: 100%;
    padding: 10px 12px;
    font: inherit;
    text-align: left;
    background: none;
    border: none;
    border-radius: 8px;
    cursor: pointer;
}

.investments button:hover {
    background: #f0f3fa;
}

.investments button.selected {
    background: #e6edfb;
    color: #2a56b8;
    font-weight: 600;
}

.details {
    margin-top: 24px;
    padding-top: 20px;
    border-top: 1px solid #e5e7eb;
}

table {
    width: 100%;
    border-collapse: collapse;
}

th,
td {
    padding: 8px 4px;
    border-bottom: 1px solid #eef0f2;
}

th {
    text-align: left;
    font-weight: normal;
    color: #6b7280;
}

td {
    text-align: right;
    font-variant-numeric: tabular-nums;
}

tr:last-child th,
tr:last-child td {
    border-bottom: none;
    font-weight: 600;
}

.muted {
    color: #6b7280;
    font-weight: normal;
}

.gain {
    color: #1a7f37;
}

.loss,
.error {
    color: #c62828;
}
</style>