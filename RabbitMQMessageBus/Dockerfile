FROM rabbitmq:3-management-alpine
COPY ./volumes/prod.conf /etc/rabbitmq/rabbitmq.conf
RUN rabbitmq-plugins enable rabbitmq_management