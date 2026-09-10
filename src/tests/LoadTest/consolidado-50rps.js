import http from 'k6/http';
import { check } from 'k6';

export const options = {
    scenarios: {
        consolidado_50rps: {
            executor: 'constant-arrival-rate',
            rate: 50,
            timeUnit: '1s',
            duration: '60s',
            preAllocatedVUs: 50,
            maxVUs: 100,
        },
    },

    thresholds: {
        http_req_failed: ['rate<0.05'],
        checks: ['rate>0.95'],
    },
};

export default function () {
    const response = http.get(
        'http://host.docker.internal:5002/consolidado-diario/2026-09-09',
        {
            headers: {
                Authorization: `Bearer ${__ENV.K6_TOKEN}`,
            },
        }
    );

    check(response, {
        'status 200': (r) => r.status === 200,
    });
}